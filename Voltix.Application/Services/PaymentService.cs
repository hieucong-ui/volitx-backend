using AutoMapper;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.DealerDebt;
using Voltix.Application.DTO.Payment;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using Voltix.Infrastructure.SignlR;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Voltix.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly string _baseUrl, _tmnCode, _hashSecret, _returnUrl;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _http;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IDealerDebtService _dealerDebtService;
        private readonly IDealerTierService _dealerTierService;
        private readonly IMapper _mapper;
        private readonly IEContractService _eContractService;
        public PaymentService(IConfiguration cfg, IUnitOfWork unitOfWork, IHttpContextAccessor httpContext, IEmailService emailService, IHubContext<NotificationHub> hubContext, IDealerDebtService dealerDebtService
            , IDealerTierService dealerTierService, IMapper mapper, IEContractService eContractService)
        {
            _baseUrl = cfg["VNPay:BaseUrl"] ?? throw new Exception("Cannot find VNPay:BaseUrl");
            _tmnCode = cfg["VNPay:TmnCode"] ?? throw new Exception("Cannot find VNPay:TmnCode");
            _hashSecret = cfg["VNPay:HashSecret"] ?? throw new Exception("Cannot find VNPay:HashSecret");
            _returnUrl = cfg["VNPay:ReturnUrl"] ?? throw new Exception("Cannot find VNPay:ReturnUrl");
            _unitOfWork = unitOfWork;
            _http = httpContext;
            _emailService = emailService;
            _hubContext = hubContext;
            _dealerDebtService = dealerDebtService;
            _dealerTierService = dealerTierService;
            _mapper = mapper;
            _eContractService = eContractService;
        }
        public async Task<ResponseDTO> CreateVNPayLink(Guid customerOrderId, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);

                if (order == null)
                {
                    return new ResponseDTO(false)
                    {
                        Message = "Order not found",
                        StatusCode = 404
                    };
                }

                decimal? amount;
                var orderNo = order.OrderNo.ToString();

                if (order.Status.Equals(OrderStatus.Confirmed) && order.DepositAmount is not null)
                {
                    amount = order.DepositAmount;
                }
                else if ((order.Status.Equals(OrderStatus.RemainingConfimmed) || order.Status.Equals(OrderStatus.RemainingPending)) && order.DepositAmount is not null)
                {
                    amount = (order.TotalAmount - order.DepositAmount);
                    orderNo = orderNo + "|" + Guid.NewGuid().ToString()[..6];
                }
                else
                {
                    amount = order.TotalAmount;
                }

                amount = amount * 100;
                var createDate = ToGmt7(DateTime.UtcNow);
                var expireDate = ToGmt7(DateTime.UtcNow.AddMinutes(15));
                var clientIp = ResolveClientIp();

                var orderInfo = order.Status == OrderStatus.DepositPending ? $"ORDER|DEPOSIT|{order.OrderNo}" :
                                order.Status == OrderStatus.Depositing ? $"ORDER|REMAIN|{order.OrderNo}" :
                                $"ORDER|FULL|{order.OrderNo}";

                var data = new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["vnp_Version"] = "2.1.0",
                    ["vnp_Command"] = "pay",
                    ["vnp_TmnCode"] = _tmnCode,
                    ["vnp_Amount"] = amount.ToString()!,
                    ["vnp_CreateDate"] = createDate,
                    ["vnp_CurrCode"] = "VND",
                    ["vnp_IpAddr"] = clientIp,
                    ["vnp_Locale"] = "vn",
                    ["vnp_OrderInfo"] = orderInfo,
                    ["vnp_OrderType"] = "240000",
                    ["vnp_ReturnUrl"] = _returnUrl,
                    ["vnp_ExpireDate"] = expireDate,
                    ["vnp_TxnRef"] = orderNo
                };

                string FormEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
                var signData = string.Join("&", data.Select(kvp => $"{kvp.Key}={FormEncode(kvp.Value)}"));
                var secureHash = HmacSha512(_hashSecret, signData);

                var queryString = signData + $"&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
                var paymentUrl = _baseUrl + "?" + queryString;

                await _emailService.NotifyPaymentLinkToCustomer(order.Customer.Email!, order.Customer.FullName!, order.OrderNo, amount.Value / 100, paymentUrl);
                return new ResponseDTO()
                {
                    Message = "VNPay link created successfully",
                    Result = paymentUrl,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to create a vnpay link: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private string HmacSha512(string secret, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private string ToGmt7(DateTime utc)
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timezone).ToString("yyyyMMddHHmmss");
            }
            catch
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timezone).ToString("yyyyMMddHHmmss");
            }
        }

        private bool IsPrivate(IPAddress ip)
        {
            var s = ip.MapToIPv4().ToString();
            if (s.StartsWith("10.") || s.StartsWith("192.168.")) return true;
            if (s.StartsWith("172."))
            {
                var parts = s.Split('.');
                if (parts.Length > 1 && int.TryParse(parts[1], out var b) && b >= 16 && b <= 31) return true;
            }
            return s == "127.0.0.1" || s == "0.0.0.0";
        }

        private string ResolveClientIp()
        {
            var http = _http.HttpContext;
            if (http == null) return "127.0.0.1";

            string? pickFirst(string? csv)
                => csv?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            var candidate =
                   pickFirst(http.Request.Headers["X-Forwarded-For"].FirstOrDefault())
                ?? http.Request.Headers["X-Real-IP"].FirstOrDefault()
                ?? http.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                ?? http.Connection.RemoteIpAddress?.ToString();

            if (!IPAddress.TryParse(candidate, out var ip))
                return "127.0.0.1";

            ip = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? ip.MapToIPv4() : ip;
            var ipv4 = ip.ToString();

            if (IsPrivate(ip))
            {
                var force = Environment.GetEnvironmentVariable("VNPAY_FORCE_CLIENT_IP");
                if (!string.IsNullOrWhiteSpace(force) && IPAddress.TryParse(force, out var forced))
                    return forced.MapToIPv4().ToString();
            }

            return ipv4;
        }

        public async Task<ResponseDTO> HandleVNPayIpn(VNPayIPNDTO ipnDTO, CancellationToken ct)
        {
            try
            {
                var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
                foreach (var prop in typeof(VNPayIPNDTO).GetProperties())
                {
                    var key = prop.Name;
                    var value = prop.GetValue(ipnDTO)?.ToString();
                    if (!string.IsNullOrEmpty(value) && key != "vnp_SecureHash")
                    {
                        data[key] = value;
                    }
                }

                string FormEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
                var signData = string.Join("&", data.Select(kvp => $"{kvp.Key}={FormEncode(kvp.Value)}"));
                var result = HmacSha512(_hashSecret, signData);

                if (!string.Equals(result, ipnDTO.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseDTO()
                    {
                        StatusCode = 200,
                        Message = "Invalid checksum",
                        Result = new { RspCode = "97", Message = "Invalid signature" }
                    };
                }

                if (ipnDTO.vnp_ResponseCode == "00" && ipnDTO.vnp_TransactionStatus == "00")
                {
                    var orderNo = ipnDTO.vnp_TxnRef;
                    if (orderNo.Contains("|"))
                    {
                        orderNo = orderNo[..^7];
                    }
                    var order = await _unitOfWork.CustomerOrderRepository.GetByOrderNoAsync(int.Parse(orderNo));
                    if (order is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Order not found.",
                            StatusCode = 404,
                            Result = new
                            {
                                RspCode = "01",
                                Message = "Order not found"
                            }
                        };
                    }

                    if (order.Status.Equals(OrderStatus.Completed))
                    {
                        return new ResponseDTO
                        {
                            StatusCode = 200,
                            Message = "Already processed",
                            Result = new
                            {
                                RspCode = "00",
                                Message = "Confirm success"
                            }
                        };
                    }

                    var paidAmount = decimal.Parse(ipnDTO.vnp_Amount) / 100;

                    var existed = await _unitOfWork.TransactionRepository.IsExistTransactionAsync("VNPay", ipnDTO.vnp_TxnRef, ct);
                    if (!existed)
                    {
                        await _unitOfWork.TransactionRepository.AddAsync(new Transaction
                        {
                            CustomerOrderId = order.Id,
                            Amount = paidAmount,
                            Provider = "VNPay",
                            OrderRef = ipnDTO.vnp_TxnRef,
                            Currency = "VND",
                            Status = TransactionStatus.Success,
                            CreatedAt = DateTime.UtcNow
                        }, ct);
                        await _unitOfWork.SaveAsync();
                    }

                    await HandleVNPayCustomerOrder(order, paidAmount, ct);
                    var orderInfo = ipnDTO.vnp_OrderInfo ?? string.Empty;

                    if (orderInfo.StartsWith("DEALERPAY|", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = orderInfo.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length < 2)
                        {
                            return new ResponseDTO
                            {
                                StatusCode = 200,
                                Message = "Invalid DEALERPAY format",
                                Result = new
                                {
                                    RspCode = "00",
                                    Message = "Confirm success (but format invalid)"
                                }
                            };
                        }

                        await HandleRecordCommission(parts, ipnDTO.vnp_TxnRef, paidAmount, ct);
                    }

                    if (order.Status.Equals(OrderStatus.Completed))
                    {
                        var dealerId = order.Quote.DealerId;

                        var effectivePolicy = await _dealerTierService.GetEffectivePolicyAsync(dealerId, ct);
                        if (effectivePolicy is null)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Cannot find effective dealer tier policy",
                                StatusCode = 404
                            };
                        }
                        var commissionRate = effectivePolicy.CommissionPercent;
                        var commissionAmount = order.TotalAmount * (commissionRate / 100);

                        await _dealerDebtService.AddCommissionForDealerAsync(dealerId, new RecordCommissionDTO
                        {
                            Amount = commissionAmount!.Value,
                            ReferenceNo = $"COMMISSION-{order.OrderNo}",
                            AtUtc = DateTime.UtcNow
                        }, ct);
                    }
                    else
                    {
                        return new ResponseDTO
                        {
                            StatusCode = 200,
                            Message = "Unknown OrderInfo",
                            Result = new
                            {
                                RspCode = "01",
                                Message = "Unknown OrderInfo"
                            }
                        };
                    }

                    await UpdateStatusRealTime(order.Quote.DealerId);
                }

                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Confirm Success",
                    Result = new
                    {
                        RspCode = "00",
                        Message = "Confirm Success"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to handle VNPay IPN: {ex.Message}",
                    StatusCode = 500,
                    Result = new
                    {
                        RspCode = "99",
                        Message = "Unknow error"
                    }
                };
            }
        }

        private async Task HandleRecordCommission(string[] parts, string vnp_TxnRef, decimal paidAmount, CancellationToken ct)
        {

            var dealerId = Guid.Parse(parts[1]);
            var refNo = parts.Length >= 3 ? parts[2] : vnp_TxnRef;

            await _dealerDebtService.AddPaymentForDealerAsync(dealerId, new RecordPaymentDTO
            {
                Amount = paidAmount,
                ReferenceNo = $"VNPay-{refNo}",
                PaidAtUtc = DateTime.UtcNow,
                Method = "Transfer"
            }, ct);

            var dealerTransaction = new Transaction
            {
                CustomerOrderId = null,
                Amount = paidAmount,
                Provider = "VNPay",
                OrderRef = vnp_TxnRef,
                Currency = "VND",
                Status = TransactionStatus.Success,
                CreatedAt = DateTime.UtcNow,
                Note = $"Dealer payment|{dealerId}"
            };
            await _unitOfWork.TransactionRepository.AddAsync(dealerTransaction, ct);
        }

        private async Task HandleVNPayCustomerOrder(CustomerOrder customerOrder, decimal amount, CancellationToken ct)
        {
            if (amount == customerOrder.TotalAmount || (customerOrder.DepositAmount != null && amount == (customerOrder.TotalAmount - customerOrder.DepositAmount)))
            {
                await HandleVehicleInOrder(customerOrder, ct);
                customerOrder.Status = OrderStatus.Completed;
                _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                await _unitOfWork.SaveAsync();
            }
            else
            {
                customerOrder.Status = OrderStatus.Depositing;
                _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                await _unitOfWork.SaveAsync();
                await _eContractService.CreateDepositEContractConfirm(customerOrder.Id, ct);
            }
        }

        private async Task HandleVehicleInOrder(CustomerOrder customerOrder, CancellationToken ct)
        {
            var outOfStock = new List<(string modelName, string versionName, string colorName, int quantity)>();
            var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(customerOrder.Quote.DealerId);
            if (warehouse is null)
            {
                throw new Exception($"Cannot find warehouse for dealerId {customerOrder.Quote.DealerId}");
            }
            foreach (var detail in customerOrder.OrderDetails)
            {
                var ev = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(detail.ElectricVehicleId);
                if (ev is null)
                {
                    throw new Exception($"Cannot find the electric vehicle in orderNo {customerOrder.OrderNo}");
                }
                ev.Status = ElectricVehicleStatus.Sold;
                _unitOfWork.ElectricVehicleRepository.Update(ev);
                var quantityCurrent = await _unitOfWork.ElectricVehicleRepository.CountDealerAvailableByVersionColorAsync(customerOrder.Quote.DealerId, ev.ElectricVehicleTemplate.VersionId,
                    ev.ElectricVehicleTemplate.ColorId, ct);

                var template = ev.ElectricVehicleTemplate;
                var version = template.Version;
                var model = version.Model;
                var color = template.Color;

                if (quantityCurrent <= warehouse.AlertNumber && !outOfStock.Any(o => o.modelName == model.ModelName && o.versionName == version.VersionName &&
                    o.colorName == color.ColorName))
                {
                    outOfStock.Add((modelName: model.ModelName ?? string.Empty,
                        versionName: version.VersionName ?? string.Empty,
                        colorName: color.ColorName ?? string.Empty,
                        quantity: quantityCurrent));
                }
            }

            if (outOfStock.Count > 0)
            {
                await CreateAggregationOutOfStockAsync(customerOrder.Quote.DealerId, outOfStock, ct);
            }
        }

        private async Task CreateAggregationOutOfStockAsync(Guid dealerId, List<(string modelName, string versionName, string colorName, int quantity)> outOfStock, CancellationToken ct)
        {
            if (outOfStock == null || outOfStock.Count == 0)
            {
                return;
            }

            var items = String.Join(", ", outOfStock.Select(i => $"{i.modelName} - {i.versionName} - {i.colorName} còn {i.quantity} xe"));

            var title = "Cảnh báo số lượng xe";
            var message = $"Lưu ý: {items}";

            var notification = new Notification
            {
                DealerId = dealerId,
                Title = title,
                Message = message,
                TargetRole = StaticUserRole.DealerManager,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.NotificationRepository.AddAsync(notification, ct);

            await _hubContext.Clients.Group($"Dealer_{dealerId}_{StaticUserRole.DealerManager}").SendAsync("NotificationChanged");
        }

        public async Task<ResponseDTO> GetAllPaymentTransaction(ClaimsPrincipal userClaim, int pageNumber, int pageSize, TransactionStatus? status, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 401
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                Expression<Func<Transaction, bool>> filter = t => t.CustomerOrder.Quote.DealerId == dealer.Id;
                if (status.HasValue)
                {
                    filter = t => t.Status == status.Value & t.CustomerOrder.Quote.DealerId == dealer.Id;
                }

                (IReadOnlyList<Transaction> items, int total) result;
                result = await _unitOfWork.TransactionRepository.GetPagedAsync(
                    filter: filter,
                    includes: null,
                    orderBy: o => o.CreatedAt,
                    ascending: false,
                    pageNumber: pageNumber,
                    pageSize: pageSize, ct);
                var getTransactionList = _mapper.Map<List<Transaction>>(result.items);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get payment transactions successfully",
                    StatusCode = 200,
                    Result = new
                    {
                        Data = getTransactionList,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to get payment transactions: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private async Task<ResponseDTO> UpdateStatusRealTime(Guid dealerId)
        {
            var groupName = $"dealer:{dealerId}:all";

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveCustomerOrderStatusUpdate");

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Real-time update customer order status sent successfully",
                StatusCode = 200
            };
        }
    }
}