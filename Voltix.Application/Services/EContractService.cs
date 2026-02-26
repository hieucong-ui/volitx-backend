using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using StackExchange.Redis;
using Voltix.Application.DTO;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EContract;
using Voltix.Application.DTO.VehicleDelivery;
using Voltix.Application.DTO.Warehouse;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Application.Pdf;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Domain.ValueObjects;
using Voltix.Infrastructure.IClient;
using Voltix.Infrastructure.IRepository;
using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;
using UglyToad.PdfPig;

namespace Voltix.Application.Services
{
    public class EContractService : IEContractService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly IVnptEContractClient _vnpt;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IS3Service _s3Service;
        private readonly IWarehouseService _warehouseService;
        private readonly IRedisService _redisService;
        public EContractService(IWarehouseService warehouseService, IConfiguration cfg, HttpClient http, IUnitOfWork unitOfWork, IVnptEContractClient vnpt,
            IEmailService emailService, IMapper mapper, IS3Service s3Service, IRedisService redisService)
        {
            _cfg = cfg;
            _http = http;
            _unitOfWork = unitOfWork;
            _vnpt = vnpt;
            _emailService = emailService;
            _mapper = mapper;
            _s3Service = s3Service;
            _warehouseService = warehouseService;
            _redisService = redisService;
        }

        public async Task<ResponseDTO<GetAccessTokenDTO>> GetAccessTokenAsync()
        {
            try
            {
                var accessToken = await _redisService.RetrieveString(StaticRedisKey.AccessTokenEVC);
                if (accessToken is null)
                {
                    var username = _cfg["EContractClient:Username"] ?? throw new Exception("Cannot find username in EContractClient");
                    var password = _cfg["EContractClient:Password"] ?? throw new Exception("Cannot find password in EContractClient");
                    int? companyId = _cfg["EContractClient:CompanyId"] is not null ? int.Parse(_cfg["EContractClient:CompanyId"]!) : throw new Exception("Cannot find company ID in EContractClient");

                    var payload = new { username, password, companyId };
                    var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });

                    var urlGetToken = $"{_cfg["EContractClient:BaseUrl"]}/api/auth/password-login";
                    using var req = new HttpRequestMessage(HttpMethod.Post, urlGetToken);
                    req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var res = await _http.SendAsync(req);
                    res.EnsureSuccessStatusCode();
                    var body = await res.Content.ReadAsStringAsync();

                    if (!res.IsSuccessStatusCode)
                        return new ResponseDTO<GetAccessTokenDTO>
                        {
                            IsSuccess = false,
                            StatusCode = (int)res.StatusCode,
                            Message = $"Cannot get access token from EContract: {body}"
                        };

                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    var dataEl = root.GetProperty("data");
                    accessToken = dataEl.ValueKind == JsonValueKind.String ? dataEl.GetString() :
                    (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("access", out var t1)) ? t1.GetString() :
                    (dataEl.ValueKind == JsonValueKind.Object && dataEl.TryGetProperty("accessToken", out var t2)) ? t2.GetString() :
                    null;

                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        return new ResponseDTO<GetAccessTokenDTO>
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = "Cannot find access token in EContract response"
                        };
                    }
                    var expiration = TimeSpan.FromDays(1) - TimeSpan.FromHours(1);
                    await _redisService.StoreKeyAsync(StaticRedisKey.AccessTokenEVC, accessToken, expiration);
                }

                var userId = int.Parse(_cfg["EContractClient:UserId"] ?? throw new Exception("Cannot find user ID in EContractClient"));

                return new ResponseDTO<GetAccessTokenDTO>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get access token successfully",
                    Data = new GetAccessTokenDTO
                    {
                        AccessToken = accessToken,
                        UserId = userId
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<GetAccessTokenDTO>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get access token: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateBookingEContractAsync(ClaimsPrincipal userClaim, Guid bookingId, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User is not login yet"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var access = await GetAccessTokenAsync();

                var created = await CreateDocumentBookingAsync(bookingId, access.Data!.AccessToken, dealer, ct);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentBookingAsync(Guid bookingId, string token, Dealer dealer, CancellationToken ct)
        {
            var templateCode = _cfg["EContract:BookingTemplateCode"] ?? throw new ArgumentNullException("EContract:DealerTemplateCode is not exist");
            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            var booking = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
            if (booking is null)
            {
                throw new Exception($"Booking with id '{bookingId}' is not exist");
            }

            string BuildBookingRowsHtml(IEnumerable<BookingEVDetail> items)
            {
                var sb = new StringBuilder();
                int i = 1;
                sb.AppendLine($@"
                        <tr>
                        <td class=""right"">Số thứ tự</td>
                        <td>Tên Model – Version</td>
                        <td>Màu</td>
                        <td class=""right"">Số lượng</td>
                        </tr>");
                foreach (var item in items)
                {
                    var modelName = item.Version?.Model?.ModelName ?? "(Mẫu)";
                    var versionName = item.Version?.VersionName ?? "(Phiên bản)";
                    var colorName = item.Color?.ColorName ?? "(Màu)";
                    var quantity = item.Quantity;

                    sb.AppendLine($@"
                        <tr>
                        <td class=""right"">{i}</td>
                        <td>{modelName} – {versionName}</td>
                        <td>{colorName}</td>
                        <td class=""right"">{quantity}</td>
                        </tr>");
                    i++;
                }
                return sb.ToString();
            }

            var rowsHtml = BuildBookingRowsHtml(booking.BookingEVDetails);
            var totalQty = booking.TotalQuantity;

            var data = new Dictionary<string, object?>
            {
                ["company.name"] = _cfg["Company:Name"] ?? "N/A",
                ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                ["dealer.name"] = dealer.Name,
                ["dealer.address"] = dealer.Address,
                ["dealer.taxNo"] = dealer.TaxNo,
                ["dealer.contact"] = $"{dealer.Manager.Email}, {dealer.Manager.PhoneNumber}",
                ["booking.date"] = booking.BookingDate.ToString("dd/MM/yyyy HH:mm"),
                ["booking.total"] = totalQty.ToString(),
                ["booking.note"] = booking.Note ?? string.Empty,
                ["booking.rows"] = rowsHtml
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);

            var pdfBytes = await EContractPdf.RenderAsync(html);
            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..6].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Booking Confirm EContract",
                Description = "EContract allows dealer confirm booking electric vehicle"
            };

            request.FileInfo.File = pdfBytes;
            var fileNameNoPdf = $"Booking_E-Contract_{randomText}_{dealer.Name}".Trim();
            var fileName = $"{fileNameNoPdf}.pdf";
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);

            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
            {
                throw new Exception("Invalid EContract status value.");
            }

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            var vnptEContractId = Guid.Parse(createResult.Data.Id);
            var status = (EContractStatus)createResult.Data!.Status!.Value;
            var eContract = new EContract(vnptEContractId, html, fileNameNoPdf, "System", dealer.ManagerId!, status, EcontractType.BookingContract);
            await _unitOfWork.EContractRepository.AddAsync(eContract, ct);

            booking.EContractId = vnptEContractId;
            booking.Status = BookingStatus.WaitingDealerSign;
            _unitOfWork.BookingEVRepository.Update(booking);
            await _unitOfWork.SaveAsync();

            return createResult;
        }

        public async Task<ResponseDTO> ConfirmBookingEVEContract(ClaimsPrincipal userClaim, Guid EContractId, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var access = await GetAccessTokenAsync();
                var token = access.Data!.AccessToken;
                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(EContractId, ct);
                if (econtract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Cannot find econtract"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(econtract.OwnerBy, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Cannot find dealer"
                    };
                }

                var vnptEcontractId = EContractId.ToString();
                var vnptEContract = await GetVnptEContractByIdAsync(vnptEcontractId, ct);

                var companyApproverUserCode = _cfg["EContractClient:CompanyApproverUserCode"] ?? throw new ArgumentNullException("EContractClient:CompanyApproverUserCode is not exist");
                await UpdateProcessAsync(token, vnptEcontractId, dealer.ManagerId!, companyApproverUserCode, vnptEContract.Data!.PositionA!, vnptEContract.Data.PositionB!, vnptEContract.Data.PageSign);

                var result = await SendProcessAsync(token, vnptEContract.Data.Id);
                var status = (EContractStatus)result.Data!.Status!.Value;

                var vnptEContractId = Guid.Parse(vnptEContract.Data.Id);
                econtract.UpdateStatus(EContractStatus.Ready);

                _unitOfWork.EContractRepository.Update(econtract);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 200,
                    Message = "Confirm booking EV successfully",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to confirm booking EV EContract: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateDepositEContractConfirm(Guid customerOderId, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer order is not exist"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(customerOrder.Quote.DealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var access = await GetAccessTokenAsync();
                var created = await CreateDepositDocumentAsync(access.Data!.AccessToken, dealer, customerOrder, ct);

                var customer = customerOrder.Customer;
                var contract = customerOrder.EContracts!.First();
                var vnptUrl = created.Data!.DownloadUrl;
                var confirmLink = StaticLinkUrl.WebUrl + $"/confirm-econtract?downloadUrl={vnptUrl}&customerOrderId={customerOderId}&email={customer.Email}";
                await _emailService.SendContractReviewAndConfirm(customer.Email!, customer.FullName!, contract.Name!, confirmLink);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        private async Task<VnptResult<VnptDocumentDto>> CreateDepositDocumentAsync(string token, Dealer dealer, CustomerOrder customerOrder, CancellationToken ct)
        {
            var templateCode = StaticEContractName.EContractDepositCustomerOrder;
            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            string BuildBookingRowsHtml(IEnumerable<QuoteDetail> items)
            {
                var sb = new StringBuilder();
                int i = 1;
                foreach (var item in items)
                {
                    var modelName = item.ElectricVehicleVersion?.Model?.ModelName ?? "(Mẫu)";
                    var versionName = item.ElectricVehicleVersion?.VersionName ?? "(Phiên bản)";
                    var colorName = item.ElectricVehicleColor?.ColorName ?? "(Màu)";
                    var quantity = item.Quantity;

                    sb.AppendLine($@"
                        <tr>
                        <td class=""right"">{i}</td>
                        <td>{modelName} – {versionName}</td>
                        <td>{colorName}</td>
                        <td class=""right"">{quantity}</td>
                        </tr>");
                    i++;
                }
                return sb.ToString();
            }

            var rowsHtml = BuildBookingRowsHtml(customerOrder.Quote.QuoteDetails);
            var transaction = await _unitOfWork.TransactionRepository.GetByCustomerOrderIdAsync(customerOrder.Id, ct);
            var method = transaction?.Provider == "Cash" ? "Tiền mặt" : "Chuyển khoản";
            var quote = customerOrder.Quote;
            var data = new Dictionary<string, object?>
            {
                ["order.no"] = customerOrder.OrderNo.ToString(),
                ["order.date"] = ToGmt7String(DateTime.UtcNow, "dd/MM/yyyy"),
                ["order.paymentMethod"] = method ?? "",

                ["dealer.name"] = quote.Dealer?.Name ?? "",
                ["dealer.address"] = quote.Dealer?.Address ?? "",
                ["dealer.taxNo"] = quote.Dealer?.TaxNo ?? "",
                ["dealer.phone"] = quote.Dealer?.Manager.PhoneNumber ?? "",
                ["dealer.email"] = quote.Dealer?.Manager.Email ?? "",
                ["dealer.bankAccount"] = quote.Dealer?.BankAccount ?? "",
                ["dealer.bankName"] = quote.Dealer?.BankName ?? "",

                ["customer.fullName"] = customerOrder.Customer?.FullName ?? "",
                ["customer.phone"] = customerOrder.Customer?.PhoneNumber ?? "",
                ["customer.email"] = customerOrder.Customer?.Email ?? "",
                ["customer.idNo"] = customerOrder.Customer?.CitizenID ?? "",
                ["customer.address"] = customerOrder.Customer?.Address ?? "",

                ["money.deposit"] = $"{(customerOrder.DepositAmount ?? 0m):#,0} VND",
                ["money.remaining"] = $"{(customerOrder.TotalAmount - (customerOrder.DepositAmount ?? 0m)):#,0} VND",
                ["money.orderTotal"] = $"{customerOrder.TotalAmount:#,0} VND",

                ["policy.holdDays"] = "15",
                ["policy.lateDays"] = "7",
                ["logistics.place"] = quote.Dealer?.Warehouse?.WarehouseName ?? "Kho Đại lý",
                ["logistics.eta"] = "Theo lịch điều phối",
                ["order.vehicleRows"] = rowsHtml,

                ["roles.A.representative"] = quote.Dealer?.Manager.FullName ?? "",
                ["roles.A.title"] = "Đại diện đại lý" ?? "",
                ["roles.A.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_A",
                ["roles.B.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_B"
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);

            var pdfBytes = await EContractPdf.RenderAsync(html);
            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..20].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Booking Confirm EContract",
                Description = "EContract confirm customer deposited"
            };

            request.FileInfo.File = pdfBytes;
            var fileNameNoPdf = $"Deposit_E-Contract_{randomText}_{dealer.Name}".Trim();
            var fileName = $"{fileNameNoPdf}.pdf";
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);

            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
            {
                throw new Exception("Invalid EContract status value.");
            }

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            var vnptEContractId = Guid.Parse(createResult.Data.Id);
            var eContract = new EContract(vnptEContractId, html, fileNameNoPdf, "System", dealer.ManagerId!, customerOrder.Id, EContractStatus.Draft, EcontractType.CustomerOrderDepositContract);

            await _unitOfWork.EContractRepository.AddAsync(eContract, ct);
            await _unitOfWork.SaveAsync();

            return createResult;
        }

        public async Task<ResponseDTO> CreatePayFullConfirmationEContract(Guid customerOderId, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer order is not exist"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(customerOrder.Quote.DealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var accessToken = await GetAccessTokenAsync();
                var created = await CreatePayFullConfirmationDraftEContract(customerOrder, dealer, ct);

                var customer = customerOrder.Customer;
                var contract = customerOrder.EContracts!.First();
                var vnptUrl = created.Data!.DownloadUrl;
                var confirmLink = StaticLinkUrl.WebUrl + $"/confirm-econtract?downloadUrl={vnptUrl}&customerOrderId={customerOderId}&email={customer.Email}";
                await _emailService.SendContractReviewAndConfirm(customer.Email!, customer.FullName!, contract.Name!, confirmLink);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Pay full confirmation EContract created successfully",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create Pay Full Confirmation EContract: {ex.Message}"
                };
            }
        }

        private async Task<VnptResult<VnptDocumentDto>> CreatePayFullConfirmationDraftEContract(CustomerOrder customerOrder, Dealer dealer, CancellationToken ct)
        {
            var hasDeposit = customerOrder.DepositAmount.HasValue && customerOrder.DepositAmount.Value > 0;

            string templateCode = hasDeposit
                ? (StaticEContractName.EContractPayRemainderCustomerOrder ?? throw new Exception("EContract:EContractPayRemainderCustomerOrderTemplateCode is not exist"))
                : (StaticEContractName.EContractPayFullCustomerOrder ?? throw new Exception("EContract:PayFullCustomerOrderTemplateCode is not exist"));

            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            string BuildRowsHtml(IEnumerable<QuoteDetail> items)
            {
                var sb = new StringBuilder();
                int i = 1;
                foreach (var item in items)
                {
                    var modelName = item.ElectricVehicleVersion?.Model?.ModelName ?? "(Mẫu)";
                    var versionName = item.ElectricVehicleVersion?.VersionName ?? "(Phiên bản)";
                    var colorName = item.ElectricVehicleColor?.ColorName ?? "(Màu)";
                    var quantity = item.Quantity;

                    sb.AppendLine($@"
                <tr>
                    <td class=""right"">{i}</td>
                    <td>{modelName} – {versionName}</td>
                    <td>{colorName}</td>
                    <td class=""right"">{quantity}</td>
                </tr>");
                    i++;
                }
                return sb.ToString();
            }

            var rowsHtml = BuildRowsHtml(customerOrder.Quote.QuoteDetails);
            var quote = customerOrder.Quote;

            var transaction = await _unitOfWork.TransactionRepository.GetByCustomerOrderIdAsync(customerOrder.Id, ct);
            var method = (transaction?.Provider == "Cash") ? "Tiền mặt" : "Chuyển khoản";

            string Vnd(decimal v) => $"{v:#,0} VND";
            decimal total = Convert.ToDecimal(customerOrder.TotalAmount);
            decimal deposited = Convert.ToDecimal(customerOrder.DepositAmount ?? 0m);
            decimal payNow = hasDeposit ? (total - deposited) : total;
            if (payNow < 0) payNow = 0m;
            decimal remainingAfterThis = Math.Max(total - (deposited + payNow), 0m);
            var data = new Dictionary<string, object?>
            {
                ["order.no"] = customerOrder.OrderNo.ToString(),
                ["order.date"] = ToGmt7String(DateTime.UtcNow, "dd/MM/yyyy"),
                ["order.paymentMethod"] = method ?? "",

                ["dealer.name"] = quote.Dealer?.Name ?? "",
                ["dealer.address"] = quote.Dealer?.Address ?? "",
                ["dealer.taxNo"] = quote.Dealer?.TaxNo ?? "",
                ["dealer.phone"] = quote.Dealer?.Manager.PhoneNumber ?? "",
                ["dealer.email"] = quote.Dealer?.Manager.Email ?? "",
                ["dealer.bankAccount"] = quote.Dealer?.BankAccount ?? "",
                ["dealer.bankName"] = quote.Dealer?.BankName ?? "",

                ["customer.fullName"] = customerOrder.Customer?.FullName ?? "",
                ["customer.phone"] = customerOrder.Customer?.PhoneNumber ?? "",
                ["customer.email"] = customerOrder.Customer?.Email ?? "",
                ["customer.idNo"] = customerOrder.Customer?.CitizenID ?? "",
                ["customer.address"] = customerOrder.Customer?.Address ?? "",

                ["money.orderTotal"] = Vnd(total),
                ["money.previousDeposit"] = Vnd(deposited),
                ["money.payNow"] = Vnd(payNow),
                ["money.deposit"] = Vnd(payNow),
                ["money.remaining"] = Vnd(remainingAfterThis),

                ["payment.type"] = hasDeposit ? "Thanh toán phần còn lại" : "Thanh toán đủ",

                ["policy.holdDays"] = "15",
                ["policy.lateDays"] = "7",
                ["logistics.place"] = quote.Dealer?.Warehouse?.WarehouseName ?? "Kho Đại lý",
                ["logistics.eta"] = "Theo lịch điều phối",
                ["order.vehicleRows"] = rowsHtml,

                ["roles.A.representative"] = quote.Dealer?.Manager.FullName ?? "",
                ["roles.A.title"] = "Đại diện đại lý",
                ["roles.A.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_A",
                ["roles.B.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_B"
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);
            var pdfBytes = await EContractPdf.RenderAsync(html);
            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"],
                            width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"],
                            width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));
            var randomText = Guid.NewGuid().ToString()[..20].ToUpper();

            var subject = hasDeposit ? "Pay Remainder Confirm EContract" : "Pay Full Confirm EContract";
            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = subject,
                Description = hasDeposit
                    ? "EContract confirm customer paid the remaining amount"
                    : "EContract confirm customer paid in full"
            };

            request.FileInfo.File = pdfBytes;
            var fileNameNoPdf = hasDeposit
                ? $"PayRemainder_Confirm_E-Contract_{randomText}_{dealer.Name}".Trim()
                : $"PayFull_Confirm_E-Contract_{randomText}_{dealer.Name}".Trim();
            var fileName = $"{fileNameNoPdf}.pdf";
            request.FileInfo.FileName = fileName;

            var access = await GetAccessTokenAsync();
            var token = access.Data!.AccessToken;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);
            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
                throw new Exception("Invalid EContract status value.");

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            var econtractType = hasDeposit
                ? EcontractType.CustomerOrderDepositFull
                : EcontractType.CustomerOrderPayFull;

            var vnptEContractId = Guid.Parse(createResult.Data.Id);
            var eContract = new EContract(vnptEContractId, html, fileNameNoPdf, "System", dealer.ManagerId!, customerOrder.Id, EContractStatus.Draft, econtractType);

            await _unitOfWork.EContractRepository.AddAsync(eContract, ct);
            await _unitOfWork.SaveAsync();

            return createResult;
        }

        public async Task<ResponseDTO> ReadyCustomerOrderEcontract(Guid eContractId, CancellationToken ct)
        {
            try
            {
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(eContractId, ct);
                if (eContract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract is not exist"
                    };
                }
                var access = await GetAccessTokenAsync();
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByEContractId(eContractId, ct);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer order is not exist"
                    };
                }

                if (customerOrder.Customer is null)
                {
                    throw new Exception("Customer is not exist");
                }

                var customer = customerOrder.Customer;

                var roleIds = new List<Guid>
                {
                    Guid.Parse(_cfg["EContract:RoleId"] ?? throw new Exception("EContract:RoleId is not exist"))
                };

                var departmentIds = new List<int>
                {
                    int.Parse(_cfg["EContract:DepartmentId"] ?? throw new Exception("EContract:DepartmentId is not exist"))
                };

                var vnptUser = new VnptUserUpsert
                {
                    Code = customer.Id.ToString(),
                    UserName = customer.Email,
                    Name = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.PhoneNumber,
                    ReceiveOtpMethod = 1,
                    ReceiveNotificationMethod = 0,
                    SignMethod = 2,
                    SignConfirmationEnabled = true,
                    GenerateSelfSignedCertEnabled = true,
                    Status = 1,
                    DepartmentIds = departmentIds,
                    RoleIds = roleIds

                };

                var vnptUserList = new[] { vnptUser };
                var token = access.Data!.AccessToken;
                var upsert = await CreateOrUpdateUsersAsync(token, vnptUserList);
                var userCode = customer.Id.ToString();

                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(customerOrder.Quote.DealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var draftEContract = await GetVnptEContractByIdAsync(eContractId.ToString(), ct);

                await UpdateProcessOrderCustomerAsync(token, eContractId.ToString(), dealer.ManagerId!, userCode, draftEContract.Data!.PositionA!, draftEContract.Data.PositionB!, draftEContract.Data.PageSign);

                var sent = await SendProcessAsync(access.Data!.AccessToken, eContractId.ToString());


                if (!Enum.IsDefined(typeof(EContractStatus), sent.Data.Status.Value))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid EContract status value.",
                    };
                }

                eContract.UpdateStatus((EContractStatus)sent.Data.Status.Value);

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Econtract ready to sign",
                    Result = sent
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        private static string ToGmt7String(DateTime utc, string format)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz).ToString(format);
            }
            catch
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz).ToString(format);
            }
        }

        public async Task<ResponseDTO> CreateDraftEContractAsync(ClaimsPrincipal userClaim, CreateDealerDTO createDealerDTO, CancellationToken ct)
        {
            try
            {
                var isExistDealer = await _unitOfWork.DealerRepository.IsExistByNameAsync(createDealerDTO.DealerName, ct);
                if (isExistDealer)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Dealer name is exist"
                    };

                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(createDealerDTO.EmailManager);

                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = createDealerDTO.EmailManager,
                        FullName = createDealerDTO.FullNameManager,
                        Email = createDealerDTO.EmailManager,
                        PhoneNumber = createDealerDTO.PhoneNumberManager,
                        LockoutEnabled = true
                    };

                    await _unitOfWork.UserManagerRepository.CreateAsync(user, "ChangeMe@" + Guid.NewGuid().ToString()[..5]);
                }

                var dealerTier = await _unitOfWork.DealerTierRepository.GetByLevelAsync(createDealerDTO.DealerLevel, ct);
                if (dealerTier is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer tier is not exist"
                    };
                }

                var dealer = new Dealer
                {
                    Id = Guid.NewGuid(),
                    ManagerId = user.Id,
                    Name = createDealerDTO.DealerName,
                    Address = createDealerDTO.DealerAddress,
                    TaxNo = createDealerDTO.TaxNo,
                    DealerTierId = dealerTier.Id,
                    BankAccount = createDealerDTO.BankAccount,
                    BankName = createDealerDTO.BankName,
                    Manager = user
                };

                var access = await GetAccessTokenAsync();

                var created = await CreateDocumentDealerAsync(userClaim, access.Data!.AccessToken, dealer, dealerTier, user, ct);

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(created.Data!.Id), ct);

                await _unitOfWork.DealerRepository.AddAsync(dealer, ct);

                var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");
                var supportEmail = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist");

                var encode = HttpUtility.UrlEncode(created.Data.DownloadUrl);
                var url = StaticLinkUrl.ViewDaftEContractURL + encode;
                await _emailService.SendContractEmailAsync(user.Email, user.FullName, created.Data!.Subject, url, created.Data.PdfBytes, created.Data.FileName, companyName, supportEmail);

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "PDF is created",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> CreateEContractAsync(ClaimsPrincipal userClaim, Guid eContractId, CancellationToken ct)
        {
            try
            {
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(eContractId, ct);
                if (eContract is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find EContract"
                    };

                var access = await GetAccessTokenAsync();

                var dealerManagerId = eContract.OwnerBy;

                var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(dealerManagerId);
                if (dealerManager is null)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "Cannot find dealer manager"
                    };

                var roleIds = new List<Guid>
                {
                    Guid.Parse(_cfg["EContract:RoleId"] ?? throw new Exception("EContract:RoleId is not exist"))
                };

                var departmentIds = new List<int>
                {
                    int.Parse(_cfg["EContract:DepartmentId"] ?? throw new Exception("EContract:DepartmentId is not exist"))
                };

                var vnptUser = new VnptUserUpsert
                {
                    Code = dealerManagerId,
                    UserName = dealerManager.Email,
                    Name = dealerManager.FullName,
                    Email = dealerManager.Email,
                    Phone = dealerManager.PhoneNumber,
                    ReceiveOtpMethod = 1,
                    ReceiveNotificationMethod = 0,
                    SignMethod = 2,
                    SignConfirmationEnabled = true,
                    GenerateSelfSignedCertEnabled = false,
                    Status = 1,
                    DepartmentIds = departmentIds,
                    RoleIds = roleIds

                };

                var vnptUserList = new[] { vnptUser };

                var upsert = await CreateOrUpdateUsersAsync(access.Data!.AccessToken, vnptUserList);

                var companyApproverUserCode = _cfg["EContractClient:CompanyApproverUserCode"] ?? throw new ArgumentNullException("EContractClient:CompanyApproverUserCode is not exist");

                var draftEContract = await GetVnptEContractByIdAsync(eContractId.ToString(), ct);
                if (!draftEContract.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Cannot get draft EContract from VNPT"
                    };
                }

                var uProcess = await UpdateProcessAsync(access.Data!.AccessToken, eContractId.ToString(), companyApproverUserCode, dealerManagerId, draftEContract.Data!.PositionA!, draftEContract.Data!.PositionB!, draftEContract.Data.PageSign);

                var sent = await SendProcessAsync(access.Data!.AccessToken, eContractId.ToString());


                if (!Enum.IsDefined(typeof(EContractStatus), sent.Data.Status.Value))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid EContract status value.",
                    };
                }

                eContract.UpdateStatus((EContractStatus)sent.Data.Status.Value);

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Econtract ready to sign",
                    Result = sent
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create EContract: {ex.Message}"
                };
            }
        }

        public static (string pos, int pageSign) GetVnptEContractPosition(byte[] pdfBytes, AnchorBox anchor, double width = 170, double height = 90,
            double offsetY = 36, double margin = 18,
            double xAdjust = 0)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);

            var page = doc.GetPage(anchor.Page);
            int lastPage = doc.NumberOfPages;

            double pw = page.Width;
            double ph = page.Height;

            double candidateLlx = Math.Clamp(anchor.Left + xAdjust, margin, pw - margin - width);
            double candidateLly = anchor.Bottom - offsetY - height;

            double availableSpaceBelowAnchor = anchor.Bottom - margin;
            double requiredSpace = offsetY + height + 20;

            bool enoughSpaceSamePage = availableSpaceBelowAnchor >= requiredSpace;

            if (enoughSpaceSamePage)
            {
                var llx = candidateLlx;
                var lly = candidateLly;
                var pos1 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos1, anchor.Page);
            }

            if (anchor.Page < lastPage)
            {
                var nextPage = doc.GetPage(anchor.Page + 1);
                double npw = nextPage.Width;
                double nph = nextPage.Height;

                double llx = Math.Clamp(anchor.Left + xAdjust, margin, npw - margin - width);
                double lly = Math.Max(nph - margin - height - 36, margin);

                var pos2 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos2, anchor.Page + 1);
            }

            {
                double llx = candidateLlx;
                double lly = margin;
                var pos3 = $"{(int)llx},{(int)lly},{(int)(llx + width)},{(int)(lly + height)}";
                return (pos3, anchor.Page);
            }
        }


        private async Task<VnptResult<VnptDocumentDto>> CreateDocumentDealerAsync(ClaimsPrincipal userClaim, string token, Dealer dealer, DealerTier dealerTier, ApplicationUser user, CancellationToken ct)
        {
            var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) throw new Exception("The user is not login yet");

            var templateCode = dealerTier.Level == 1 ? StaticEContractName.EContractDealerTier1 :
                               dealerTier.Level == 2 ? StaticEContractName.EContractDealerTier2 :
                               dealerTier.Level == 3 ? StaticEContractName.EContractDealerTier3 :
                               dealerTier.Level == 4 ? StaticEContractName.EContractDealerTier4 :
                               dealerTier.Level == 5 ? StaticEContractName.EContractDealerTier5 :
                               throw new Exception("Invalid dealer tier level");

            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            var companyName = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist");
            var supportEmail = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist");
            var data = new Dictionary<string, object?>
            {
                ["company.name"] = companyName,
                ["company.address"] = _cfg["Company:Address"] ?? "N/A",
                ["company.taxNo"] = _cfg["Company:TaxNo"] ?? "N/A",
                ["company.phone"] = "0326336224",
                ["company.email"] = supportEmail,
                ["company.bankAccount"] = "TPBank",
                ["company.bankName"] = "0326336224",

                ["dealer.name"] = dealer.Name,
                ["dealer.address"] = dealer.Address,
                ["dealer.taxNo"] = dealer.TaxNo,
                ["dealer.contact"] = $"{user.Email}, {user.PhoneNumber}",
                ["dealer.phone"] = $"{user.PhoneNumber}",
                ["dealer.email"] = $"{user.Email}",
                ["dealer.bankAccount"] = $"{dealer.BankAccount}",
                ["dealer.bankName"] = $"{dealer.BankName}",

                ["contract.date"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                ["contract.effectiveDate"] = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                ["contract.expiryDate"] = DateTime.UtcNow.AddDays(365).ToString("dd/MM/yyyy"),

                ["dealer.tier.name"] = dealerTier?.Name ?? "N/A",
                ["dealer.tier.level"] = dealerTier?.Level.ToString() ?? "0",
                ["dealer.tier.description"] = dealerTier?.Description ?? "N/A",
                ["dealer.tier.baseCommissionPercent"] = dealerTier?.BaseCommissionPercent?.ToString() ?? "0",
                ["dealer.tier.baseCreditLimit"] = dealerTier?.BaseCreditLimit?.ToString() ?? "0",
                ["dealer.tier.baseDepositPercent"] = dealerTier?.BaseDepositPercent?.ToString() ?? "0",
                ["dealer.tier.baseLatePenaltyPercent"] = dealerTier?.BaseLatePenaltyPercent?.ToString() ?? "0",
                ["dealer.tier.createdAt"] = (dealerTier?.CreatedAt ?? DateTime.UtcNow).ToString("dd/MM/yyyy"),
                ["dealer.tier.updatedAt"] = dealerTier?.UpdatedAt?.ToString("dd/MM/yyyy") ?? "",

                ["roles.A.representative"] = "Trần Đức Hiệu", // Placeholder for company representative
                ["roles.A.title"] = "Giám đốc",
                ["roles.A.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_A",

                ["roles.B.representative"] = user.FullName,
                ["roles.B.title"] = "Đại lý",
                ["roles.B.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_B",
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);

            var pdfBytes = await EContractPdf.RenderAsync(html);

            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..6].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = $"Dealer Contract",
                Description = "Contract allows customers to open dealer"
            };

            request.FileInfo.File = pdfBytes;
            var fileNameNoPdf = $"Dealer_E-Contract_{randomText}_{dealer.Name}".Trim();
            var fileName = $"{fileNameNoPdf}.pdf";
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);


            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
            {
                throw new Exception("Invalid EContract status value.");
            }

            var status = (EContractStatus)createResult.Data.Status.Value;

            var EContract = new EContract(Guid.Parse(createResult.Data.Id), html, fileNameNoPdf, userId, user.Id, status, EcontractType.DealerContract);

            await _unitOfWork.EContractRepository.AddAsync(EContract, ct);

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            return createResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> UpdateProcessOrderCustomerAsync(string token, string documentId, string userCodeFirst, string userCodeSeccond, string positionA, string positionB, int pageSign)
        {
            var request = new VnptUpdateProcessDTO
            {
                Id = documentId,
                ProcessInOrder = true,
                Processes =
                [
                    new (orderNo:1, processedByUserCode:userCodeFirst, accessPermissionCode:"D", position: positionA, pageSign: pageSign),
                        new (orderNo:2, processedByUserCode:userCodeSeccond, accessPermissionCode:"E", position: positionB, pageSign: pageSign)
                ]
            };

            var uProcessResult = await _vnpt.UpdateProcessAsync(token, request);
            return uProcessResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> UpdateProcessAsync(string token, string documentId, string userCodeFirst, string userCodeSeccond, string positionA, string positionB, int pageSign)
        {
            var request = new VnptUpdateProcessDTO
            {
                Id = documentId,
                ProcessInOrder = true,
                Processes =
                [
                    new (orderNo:1, processedByUserCode:userCodeFirst, accessPermissionCode:"D", position: positionA, pageSign: pageSign),
                        new (orderNo:2, processedByUserCode:userCodeSeccond, accessPermissionCode:"D", position: positionB, pageSign: pageSign)
                ]
            };

            var uProcessResult = await _vnpt.UpdateProcessAsync(token, request);
            return uProcessResult;
        }

        private async Task<VnptResult<VnptDocumentDto>> SendProcessAsync(string token, string documentId)
            => await _vnpt.SendProcessAsync(token, documentId);


        private async Task<VnptResult<List<VnptUserDto>>> CreateOrUpdateUsersAsync(string token, IEnumerable<VnptUserUpsert> users)
           => await _vnpt.CreateOrUpdateUsersAsync(token, users);


        public Task<byte[]> DownloadAsync(string url)
          => _vnpt.DownloadAsync(url);

        public async Task<ResponseDTO> SignProcess(string token, VnptProcessDTO vnptProcessDTO, CancellationToken ct)
        {
            try
            {
                var request = new VnptProcessDTO
                {
                    ProcessId = vnptProcessDTO.ProcessId,
                    Reason = vnptProcessDTO.Reason,
                    Reject = vnptProcessDTO.Reject,
                    Otp = vnptProcessDTO.Otp,
                    SignatureDisplayMode = vnptProcessDTO.SignatureDisplayMode,
                    SignatureImage = vnptProcessDTO.SignatureImage,
                    SigningPage = vnptProcessDTO.SigningPage,
                    SigningPosition = vnptProcessDTO.SigningPosition,
                    SignatureText = vnptProcessDTO.SignatureText,
                    FontSize = vnptProcessDTO.FontSize,
                    ShowReason = vnptProcessDTO.ShowReason,
                    ConfirmTermsConditions = vnptProcessDTO.ConfirmTermsConditions,
                };

                var signResult = await _vnpt.SignProcess(token, request);

                if (signResult.Data?.Status is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = string.Join("; ", signResult.Messages)
                    };
                }

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(signResult.Data.Id, ct);
                if (econtract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found.",
                    };
                }

                if (!Enum.IsDefined(typeof(EContractStatus), signResult.Data.Status.Value))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid EContract status value.",
                    };
                }

                if (!signResult.Success)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = signResult.Code == 0 ? 500 : 200,
                        Message = $"Error to digital sign: {string.Join(", ", signResult.Messages)}",
                        Result = signResult
                    };
                }

                if (signResult.Data.Status.Value is (int)EContractStatus.Completed && econtract.Type is EcontractType.DealerContract)
                {
                    await CreateDealerAccount(signResult.Data.Id.ToString(), ct);
                }
                else if (signResult.Data.Status.Value is (int)EContractStatus.InProgress && econtract.Type is EcontractType.BookingContract)
                {
                    econtract.BookingEV!.Status = BookingStatus.Pending;
                }
                else if (signResult.Data.Status.Value is (int)EContractStatus.Completed && econtract.Type is EcontractType.BookingContract)
                {
                    await UpdateBookingStatusAfterSignAsync(econtract.BookingEV!.Id, ct);
                    econtract.BookingEV.Status = BookingStatus.SignedByAdmin;
                }
                else if (signResult.Data.Status.Value is (int)EContractStatus.Completed && econtract.Type is EcontractType.CustomerOrderVinConfirm)
                {
                    econtract.BookingEV!.Status = BookingStatus.Completed;
                    econtract.BookingEV!.VehicleDelivery.Status = DeliveryStatus.Confirmed;
                    _unitOfWork.VehicleDeliveryRepository.Update(econtract.BookingEV.VehicleDelivery);
                    _unitOfWork.BookingEVRepository.Update(econtract.BookingEV);
                }

                econtract.UpdateStatus((EContractStatus)signResult.Data.Status.Value);
                _unitOfWork.EContractRepository.Update(econtract);

                if (econtract.BookingEV is not null && econtract.Type is EcontractType.BookingContract)
                {
                    _unitOfWork.BookingEVRepository.Update(econtract.BookingEV);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Success to digital sign",
                    Result = signResult
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to digital sign: {ex.Message}"
                };
            }
        }

        private async Task CreateDealerAccount(string documentId, CancellationToken ct)
        {
            var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(documentId), ct);
            if (eContract is null) throw new Exception($"Cannot find EContract with id '{documentId}'");

            var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.OwnerBy);
            if (dealerManager is null) throw new Exception($"Cannot find dealer manager with id '{eContract.OwnerBy}'");

            var password = "Dealer@" + Guid.NewGuid().ToString()[..6];

            dealerManager.EmailConfirmed = true;
            dealerManager.PhoneNumberConfirmed = true;
            dealerManager.LockoutEnabled = false;
            _unitOfWork.UserManagerRepository.Update(dealerManager);

            var addToRoleResult = await _unitOfWork.UserManagerRepository.AddToRoleAsync(dealerManager, StaticUserRole.DealerManager);
            if (addToRoleResult is null) throw new Exception($"Cannot add dealer manager to role '{StaticUserRole.DealerManager}'");

            await _unitOfWork.UserManagerRepository.SetPassword(dealerManager, password);
            var data = new Dictionary<string, string>
            {
                ["{FullName}"] = dealerManager.FullName!,
                ["{UserName}"] = dealerManager.Email!,
                ["{Password}"] = password,
                ["{LoginUrl}"] = StaticLinkUrl.WebUrl,
                ["{Company}"] = _cfg["Company:Name"] ?? throw new ArgumentNullException("Company:Name is not exist"),
                ["{SupportEmail}"] = _cfg["Company:Email"] ?? throw new ArgumentNullException("Company:Email is not exist")
            };
            await _emailService.SendEmailFromTemplate(dealerManager.Email, "DealerWelcome", data);

            var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(dealerManager.Id, ct);
            if (dealer is null) throw new Exception($"Cannot find dealer with manager id '{dealerManager.Id}'");
            dealer.DealerStatus = DealerStatus.Active;
            _unitOfWork.DealerRepository.Update(dealer);

            var warehouse = new CreateWarehouseDTO
            {
                DealerId = dealer.Id,
                EVCInventoryId = null,
                WarehouseType = WarehouseType.Dealer,
                WarehouseName = $"Kho {dealer.Name}"
            };

            await _warehouseService.CreateWarehouseAsync(warehouse);
        }

        public async Task<HttpResponseMessage> GetPreviewResponseAsync(string downloadUrl, string? rangeHeader = null, CancellationToken ct = default)
        {
            try
            {
                return await _vnpt.GetDownloadResponseAsync(downloadUrl, rangeHeader, ct);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error to get preview response: {ex.Message}");
            }
        }

        public async Task<ProcessLoginInfoDto> GetAccessTokenAsyncByCode(string processCode, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(processCode))
                throw new ArgumentException("processCode is required", nameof(processCode));

            var url = $"{_cfg["EContractClient:BaseUrl"]}/api/auth/process-code-login";
            var payload = new { processCode };

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using var res = await _http.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {(int)res.StatusCode} {res.ReasonPhrase}\n{req.Method} {req.RequestUri}\n{body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            JsonElement dataEl;
            if (root.TryGetProperty("data", out var dataProp) &&
                dataProp.ValueKind == JsonValueKind.Object)
            {
                dataEl = dataProp;
            }
            else if (root.TryGetProperty("token", out var tokenProp) &&
                     root.TryGetProperty("document", out var docProp))
            {
                dataEl = root;
            }
            else
            {
                throw new Exception("Unexpected response format: " + body);
            }

            dataEl = root.GetProperty("data");
            string? accessToken = null;
            if (dataEl.TryGetProperty("token", out var tokenEl))
            {
                if (tokenEl.ValueKind == JsonValueKind.Object &&
                    tokenEl.TryGetProperty("accessToken", out var atEl) &&
                    atEl.ValueKind == JsonValueKind.String)
                {
                    accessToken = atEl.GetString();
                }
                else if (tokenEl.ValueKind == JsonValueKind.String)
                {
                    accessToken = tokenEl.GetString();
                }
            }

            var docEl = dataEl.GetProperty("document");

            string? waitingProcessId = null;
            int? processedByUserId = null;
            string? downloadUrl = null;
            string? position = null;
            int? pageSign = null;
            bool isOTP = false;

            if (docEl.TryGetProperty("waitingProcess", out var waitingEl))
            {
                if (waitingEl.TryGetProperty("id", out var idPro) && idPro.ValueKind == JsonValueKind.String)
                    waitingProcessId = idPro.GetString();

                if (waitingEl.TryGetProperty("processedByUserId", out var pEl) && pEl.ValueKind == JsonValueKind.Number)
                    processedByUserId = pEl.GetInt32();

                if (waitingEl.TryGetProperty("position", out var psEl) && psEl.ValueKind == JsonValueKind.String)
                    position = psEl.GetString();

                if (waitingEl.TryGetProperty("pageSign", out var pageEl) && pageEl.ValueKind == JsonValueKind.Number)
                    pageSign = pageEl.GetInt32();

                if (waitingEl.TryGetProperty("accessPermission", out var apEl))
                {
                    if (apEl.TryGetProperty("value", out var vlEl) && vlEl.ValueKind == JsonValueKind.Number)
                    {
                        if (vlEl.GetInt32() == 7)
                        {
                            isOTP = true;
                        }
                    }
                }
            }

            if (docEl.TryGetProperty("downloadUrl", out var down) && down.ValueKind == JsonValueKind.String)
                downloadUrl = down.GetString();

            return new ProcessLoginInfoDto
            {
                ProcessId = waitingProcessId,
                DownloadUrl = downloadUrl,
                ProcessedByUserId = processedByUserId,
                AccessToken = accessToken,
                Position = position,
                PageSign = pageSign,
                IsOTP = isOTP
            };
        }

        public async Task<VnptResult<VnptSmartCAResponse>> AddSmartCA(AddNewSmartCADTO addNewSmartCADTO)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.AddSmartCA(access.Data!.AccessToken, addNewSmartCADTO);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to add SmartCA: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptSmartCAResponse>($"Exception when adding SmartCA: {ex.Message}");
            }
        }

        public async Task<VnptResult<VnptFullUserData>> GetSmartCAInformation(int userId)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetSmartCAInformation(access.Data!.AccessToken, userId);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get SmartCA information: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptFullUserData>($"Exception when getting SmartCA information: {ex.Message}");
            }
        }

        public async Task<VnptResult<VnptSmartCAResponse>> UpdateSmartCA(UpdateSmartDTO updateSmartDTO)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.UpdateSmartCA(access.Data!.AccessToken, updateSmartDTO);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to update SmartCA: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptSmartCAResponse>($"Exception when updating SmartCA: {ex.Message}");
            }
        }

        public async Task<VnptResult<UpdateEContractResponse>> UpdateEContract(ClaimsPrincipal userClaim, UpdateEContractDTO updateEContractDTO, CancellationToken ct)
        {
            try
            {
                var Role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var contract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(updateEContractDTO.Id), ct);
                if (contract is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find EContract with id '{updateEContractDTO.Id}'");

                var isAdminOrStaff = Role == StaticUserRole.Admin || Role == StaticUserRole.EVMStaff;
                var isCreator = contract.CreatedBy != userId &&
                    (contract.Type == EcontractType.CustomerOrderPayFull ||
                    contract.Type == EcontractType.CustomerOrderDepositFull ||
                    contract.Type == EcontractType.CustomerOrderDepositContract);
                if (!isAdminOrStaff && !isCreator)
                    return new VnptResult<UpdateEContractResponse>($"You do not have permission to update this EContract");

                var access = await GetAccessTokenAsync();
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(contract.OwnerBy, ct);
                if (dealer is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find dealer with manager id '{contract.OwnerBy}'");

                var dealerManager = await _unitOfWork.UserManagerRepository.GetByIdAsync(contract.OwnerBy);
                if (dealerManager is null)
                    return new VnptResult<UpdateEContractResponse>($"Cannot find dealer manager with id '{contract.OwnerBy}'");

                var html = updateEContractDTO.HtmlFile;

                var filePdf = await EContractPdf.RenderAsync(html);

                var formFile = new FormFile(
                    new MemoryStream(filePdf), 0, filePdf.Length, "file", updateEContractDTO.Subject + ".pdf"
                );

                var response = await _vnpt.UpdateEContract(access.Data!.AccessToken, updateEContractDTO.Id, updateEContractDTO.Subject, formFile);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to update EContract: {errors}");
                }

                contract.UpdateHtmlTemplate(html, updateEContractDTO.Subject);
                _unitOfWork.EContractRepository.Update(contract);
                await _unitOfWork.SaveAsync();

                var anchors = EContractPdf.FindAnchors(filePdf, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });
                var positionA = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
                var positionB = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);
                response.Data!.PositionA = positionA.Item1;
                response.Data.PositionB = positionB.Item1;
                response.Data.PageSign = positionA.Item2;

                var updatedAt = DateTime.UtcNow.ToString("HH:mm:ss dd/MM/yyyy");
                if (contract.Type == EcontractType.DealerContract || contract.Type == EcontractType.BookingContract)
                {
                    await _emailService.NotifyEContractUpdated(dealerManager.Email!, dealerManager.FullName!, updatedAt, response.Data.DownloadUrl!);
                }
                else
                {
                    if (contract.CustomerOrder is null)
                        return new VnptResult<UpdateEContractResponse>($"EContract with id '{updateEContractDTO.Id}' does not link to any customer order");
                    if (contract.CustomerOrder.Customer is null)
                        return new VnptResult<UpdateEContractResponse>($"Customer order with id '{contract.CustomerOrder.Id}' does not link to any customer");
                    if (string.IsNullOrWhiteSpace(contract.CustomerOrder.Customer.Email))
                        return new VnptResult<UpdateEContractResponse>($"Customer with id '{contract.CustomerOrder.Customer.Id}' does not have email");
                    if (string.IsNullOrWhiteSpace(contract.CustomerOrder.Customer.FullName))
                        return new VnptResult<UpdateEContractResponse>($"Customer with id '{contract.CustomerOrder.Customer.Id}' does not have full name");

                    await _emailService.NotifyEContractUpdated(contract.CustomerOrder.Customer.Email, contract.CustomerOrder.Customer.FullName, updatedAt, response.Data.DownloadUrl!);
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<UpdateEContractResponse>($"Exception when updating EContract: {ex.Message}");
            }
        }

        public async Task<ResponseDTO<EContract>> GetAllEContractList(ClaimsPrincipal userClaim, int? pageNumber, int? pageSize, EContractStatus eContractStatus = default,
            EcontractType econtractType = default, CancellationToken ct = default)
        {
            try
            {
                var role = userClaim.FindFirst(ClaimTypes.Role)?.Value;
                if (role is null)
                {
                    return new ResponseDTO<EContract>
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "Cannot find user role"
                    };
                }

                var eContractList = await _unitOfWork.EContractRepository.GetAllAsync(includes: e => e.Include(inc => inc.Owner));

                if (role.Equals(StaticUserRole.DealerManager) || role.Equals(StaticUserRole.DealerStaff))
                {
                    var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId is null)
                    {
                        return new ResponseDTO<EContract>
                        {
                            IsSuccess = false,
                            StatusCode = 401,
                            Message = "Cannot find user ID"
                        };
                    }

                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO<EContract>
                        {
                            IsSuccess = false,
                            StatusCode = 404,
                            Message = "Cannot find dealer for the user"
                        };
                    }
                    eContractList = eContractList.Where(ec => ec.OwnerBy == dealer.ManagerId);
                }

                if (eContractStatus != default)
                {
                    eContractList = eContractList.Where(ec => ec.Status == eContractStatus).OrderByDescending(ec => ec.CreatedAt);
                }

                if (econtractType != default && !econtractType.Equals(EcontractType.CustomerOrderDepositContract))
                {
                    eContractList = eContractList.Where(ec => ec.Type == econtractType).OrderByDescending(ec => ec.CreatedAt);
                }

                if (econtractType != default && econtractType.Equals(EcontractType.CustomerOrderDepositContract))
                {
                    eContractList = eContractList.Where(ec => ec.Type == econtractType || ec.Type == EcontractType.CustomerOrderPayFull || ec.Type == EcontractType.CustomerOrderDepositFull).OrderByDescending(ec => ec.CreatedAt);
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    eContractList = eContractList.Skip(((int)pageNumber - 1) * (int)pageSize).Take((int)pageSize).ToList();
                }
                else
                {
                    return new ResponseDTO<EContract>
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "pageNumber and pageSize must be greater than 0"
                    };
                }

                var getList = _mapper.Map<List<GetEContractDTO>>(eContractList);

                foreach (var eContract in getList)
                {
                    eContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.CreatedBy))?.FullName;
                }
                return new ResponseDTO<EContract>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get EContract list successfully",
                    Result = getList
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<EContract>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get EContract list: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<EContract>> GetEContractByIdAsync(string eContractId, CancellationToken ct)
        {
            try
            {
                var eContract = await _unitOfWork.EContractRepository.GetByIdAsync(Guid.Parse(eContractId), ct);
                if (eContract is null)
                {
                    return new ResponseDTO<EContract>
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found"
                    };
                }


                var getEContract = _mapper.Map<GetEContractDTO>(eContract);
                getEContract.CreatedName = (await _unitOfWork.UserManagerRepository.GetByIdAsync(eContract.CreatedBy))?.FullName;
                return new ResponseDTO<EContract>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get EContract successfully",
                    Result = getEContract
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO<EContract>
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get EContract by id: {ex.Message}"
                };
            }
        }

        public async Task<VnptResult<VnptDocumentDto>> GetVnptEContractByIdAsync(string eContractId, CancellationToken ct)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractByIdAsync(access.Data!.AccessToken, eContractId);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get EContract by id: {errors}");
                }

                var filePdf = await _vnpt.DownloadAsync(response.Data!.DownloadUrl!);
                var anchors = EContractPdf.FindAnchors(filePdf, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });
                var positionA = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);
                var positionB = GetVnptEContractPosition(filePdf, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);
                response.Data.PositionA = positionA.Item1;
                response.Data.PositionB = positionB.Item1;
                response.Data.PageSign = positionA.Item2;
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<VnptDocumentDto>($"Exception when get EContract by id: {ex.Message}");
            }
        }

        public async Task<VnptResult<GetEContractResponse<DocumentListItemDto>>> GetAllVnptEContractList(int? pageNumber, int? pageSize, EContractStatus eContractStatus)
        {
            try
            {
                var access = await GetAccessTokenAsync();
                var response = await _vnpt.GetEContractList(access.Data!.AccessToken, pageNumber, pageSize, eContractStatus);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to get all vnpt EContract: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<GetEContractResponse<DocumentListItemDto>>($"Exception when get all vnpt EContract: {ex.Message}");
            }
        }

        public async Task<ResponseDTO> DeleteEContractDraft(Guid EContractId, CancellationToken ct)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var econtract = await _unitOfWork.EContractRepository.GetByIdAsync(EContractId, ct);
                if (econtract is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EContract not found.",
                    };
                }

                if (econtract.Status != EContractStatus.Draft)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Only EContract with Draft status can be deleted.",
                    };
                }

                var deleteResult = await _vnpt.DeleteEContractDraft(token.Data!.AccessToken, EContractId);

                if (deleteResult.Data!.Status!.Value == (int)EContractStatus.Draft)
                {
                    var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(econtract.OwnerBy, ct);
                    var manager = dealer!.Manager;
                    if (manager!.LockoutEnabled is true)
                    {
                        _unitOfWork.UserManagerRepository.Remove(manager);
                    }
                    _unitOfWork.EContractRepository.Remove(econtract);
                    _unitOfWork.DealerRepository.Remove(dealer);
                    await _unitOfWork.SaveAsync();

                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "EContract draft deleted successfully"
                    };
                }

                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = "Error to delete EContract draft that status not correct."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to delete EContract draft: {ex.Message}"
                };
            }
        }

        public async Task<VnptResult<DeleteSmartCAResponse>> DeleteSmartCA(DeleteSmartCARequest deleteSmartCARequest)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                var response = await _vnpt.DeleteSmartCA(token.Data!.AccessToken, deleteSmartCARequest);
                if (!response.Success)
                {
                    var errors = string.Join(", ", response.Messages);
                    throw new Exception($"Error to delete SmartCA: {errors}");
                }
                return response;
            }
            catch (Exception ex)
            {
                return new VnptResult<DeleteSmartCAResponse>($"Exception when deleting SmartCA: {ex.Message}");
            }
        }

        private async Task UpdateBookingStatusAfterSignAsync(Guid bookingId, CancellationToken ct)
        {
            var bookingEV = await _unitOfWork.BookingEVRepository.GetBookingWithIdAsync(bookingId);
            if (bookingEV == null)
            {
                throw new Exception("Booking not found.");
            }

            if (bookingEV.Status != BookingStatus.Approved)
            {
                throw new Exception("Can only sign an approved booking.");
            }

            await CreateVehicleDeliveryAsync(bookingEV, ct);
        }

        private async Task<ResponseDTO> CreateVehicleDeliveryAsync(BookingEV bookingEV, CancellationToken ct)
        {
            var vehicleDelivery = new VehicleDelivery
            {
                BookingEVId = bookingEV.Id,
                Description = "Preparing vehicles to delivery",
                CreatedDate = DateTime.UtcNow,
                Status = DeliveryStatus.Preparing,
                UpdateAt = DateTime.UtcNow,
            };

            await _unitOfWork.VehicleDeliveryRepository.AddAsync(vehicleDelivery, ct);
            await _unitOfWork.SaveAsync();
            foreach (var dt in bookingEV.BookingEVDetails)
            {
                var bookedVehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetBookedVehicleByModelVersionColorAsync(dt.Version.ModelId, dt.VersionId, dt.ColorId);

                if (bookedVehicles.Count() < dt.Quantity)
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = $"Not enough booked vehicles ",
                        StatusCode = 400
                    };

                var selectedVehicles = bookedVehicles
                    .OrderBy(ev => ev.ImportDate)
                    .Take(dt.Quantity)
                    .ToList();

                foreach (var ev in selectedVehicles)
                {
                    ev.Status = ElectricVehicleStatus.InTransit;
                    _unitOfWork.ElectricVehicleRepository.Update(ev);

                    var deliveryDetail = new VehicleDeliveryDetail
                    {
                        VehicleDeliveryId = vehicleDelivery.Id,
                        ElectricVehicleId = ev.Id,
                        Status = DeliveryVehicleStatus.Preparing,
                        Note = "Vehicle is being prepared for shipment"
                    };
                    await _unitOfWork.VehicleDeliveryDetailRepository.AddAsync(deliveryDetail, ct);
                }
            }
            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Create Vehicle Delivery successfully",
                StatusCode = 200
            };
        }

        public async Task<ResponseDTO> CreateEContractInvoiceConfirmBookingEV(Guid customerOrderId, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer order is not exist"
                    };
                }

                if (customerOrder.Customer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer is not exist"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(customerOrder.Quote.DealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer is not exist"
                    };
                }

                var access = await GetAccessTokenAsync();
                var created = await CreateInvoiceConfirmDocumentAsync(
                    access.Data!.AccessToken,
                    dealer,
                    customerOrder,
                    ct
                );

                var customer = customerOrder.Customer;
                var contract = customerOrder.EContracts!.FirstOrDefault(ec =>
                    ec.Type == EcontractType.CustomerOrderVinConfirm)
                    ?? customerOrder.EContracts!.First();

                var vnptUrl = created.Data!.DownloadUrl;
                var confirmLink = StaticLinkUrl.WebUrl +
                                  $"/confirm-econtract?downloadUrl={vnptUrl}&customerOrderId={customerOrderId}&email={customer.Email}";

                await _emailService.SendContractReviewAndConfirm(customer.Email!, customer.FullName!, contract.Name!, confirmLink
                );

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "VIN confirmation EContract created successfully",
                    Result = created
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create VIN Confirmation EContract: {ex.Message}"
                };
            }
        }


        private async Task<VnptResult<VnptDocumentDto>> CreateInvoiceConfirmDocumentAsync(string token, Dealer dealer, CustomerOrder customerOrder, CancellationToken ct)
        {
            var templateCode = StaticEContractName.EContractInvoiceConfirmBookingEV;
            var template = await _unitOfWork.EContractTemplateRepository.GetbyCodeAsync(templateCode, ct);
            if (template is null) throw new Exception($"Template with code '{templateCode}' is not exist");

            string BuildVinRowsHtml(IEnumerable<OrderDetail> items)
            {
                var sb = new StringBuilder();
                int i = 1;
                foreach (var item in items)
                {
                    var ev = item.ElectricVehicle;
                    var modelName = ev?.ElectricVehicleTemplate.Version?.Model?.ModelName ?? "(Model)";
                    var versionName = ev?.ElectricVehicleTemplate.Version?.VersionName ?? "(Version)";
                    var colorName = ev?.ElectricVehicleTemplate.Color?.ColorName ?? "(Màu)";
                    var vin = ev?.VIN ?? "(VIN)";

                    sb.AppendLine($@"
                <tr>
                    <td class=""right"">{i}</td>
                    <td>{modelName} – {versionName}</td>
                    <td>{colorName}</td>
                    <td>{vin}</td>
                </tr>");
                    i++;
                }
                return sb.ToString();
            }

            var rowsHtml = BuildVinRowsHtml(customerOrder.OrderDetails);
            var quote = customerOrder.Quote;
            var customer = customerOrder.Customer;

            var data = new Dictionary<string, object?>
            {
                ["order.no"] = customerOrder.OrderNo.ToString(),
                ["order.date"] = ToGmt7String(DateTime.UtcNow, "dd/MM/yyyy"),

                ["dealer.name"] = quote.Dealer?.Name ?? "",
                ["dealer.address"] = quote.Dealer?.Address ?? "",
                ["dealer.taxNo"] = quote.Dealer?.TaxNo ?? "",
                ["dealer.phone"] = quote.Dealer?.Manager.PhoneNumber ?? "",
                ["dealer.email"] = quote.Dealer?.Manager.Email ?? "",

                ["customer.fullName"] = customer?.FullName ?? "",
                ["customer.phone"] = customer?.PhoneNumber ?? "",
                ["customer.email"] = customer?.Email ?? "",
                ["customer.idNo"] = customer?.CitizenID ?? "",
                ["customer.address"] = customer?.Address ?? "",

                ["order.vehicleVinRows"] = rowsHtml,

                ["roles.A.representative"] = quote.Dealer?.Manager.FullName ?? "",
                ["roles.A.title"] = "Đại diện đại lý",
                ["roles.A.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_A",

                ["roles.B.representative"] = customer?.FullName ?? "",
                ["roles.B.title"] = "Khách hàng",
                ["roles.B.signatureAnchor"] = "ĐẠI_DIỆN_BÊN_B"
            };

            var html = EContractPdf.ReplacePlaceholders(template.ContentHtml, data, htmlEncode: false);
            var pdfBytes = await EContractPdf.RenderAsync(html);

            var anchors = EContractPdf.FindAnchors(pdfBytes, new[] { "ĐẠI_DIỆN_BÊN_A", "ĐẠI_DIỆN_BÊN_B" });

            var positionA = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_A"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: -28);

            var positionB = GetVnptEContractPosition(pdfBytes, anchors["ĐẠI_DIỆN_BÊN_B"], width: 170, height: 90, offsetY: 60, margin: 18, xAdjust: 0);

            var documentTypeId = int.Parse(_cfg["EContract:DocumentTypeId"] ?? throw new NullReferenceException("EContract:DocumentTypeId is not exist"));
            var departmentId = int.Parse(_cfg["EContract:DepartmentId"] ?? throw new NullReferenceException("EContract:DepartmentId is not exist"));

            var randomText = Guid.NewGuid().ToString()[..20].ToUpper();

            var request = new CreateDocumentDTO
            {
                TypeId = documentTypeId,
                DepartmentId = departmentId,
                No = $"EContract-{randomText}",
                Subject = "VIN Confirm EContract",
                Description = "EContract confirm VIN numbers for customer order"
            };

            request.FileInfo.File = pdfBytes;
            var fileNameNoPdf = $"VIN_Confirm_E-Contract_{randomText}_{dealer.Name}".Trim();
            var fileName = $"{fileNameNoPdf}.pdf";
            request.FileInfo.FileName = fileName;

            var createResult = await _vnpt.CreateDocumentAsync(token, request);
            if (!Enum.IsDefined(typeof(EContractStatus), createResult.Data.Status.Value))
                throw new Exception("Invalid EContract status value.");

            createResult.Data!.PositionA = positionA.Item1;
            createResult.Data.PositionB = positionB.Item1;
            createResult.Data.PageSign = positionA.Item2;
            createResult.Data.FileName = request.FileInfo.FileName;

            var vnptEContractId = Guid.Parse(createResult.Data.Id);
            var eContract = new EContract(
                vnptEContractId,
                html,
                fileNameNoPdf,
                "System",
                dealer.ManagerId!,
                customerOrder.Id,
                EContractStatus.Draft,
                EcontractType.CustomerOrderVinConfirm
            );

            await _unitOfWork.EContractRepository.AddAsync(eContract, ct);
            await _unitOfWork.SaveAsync();

            return createResult;
        }

    }
}

