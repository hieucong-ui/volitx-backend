using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.CustomerOrder;
using Voltix.Application.DTO.DealerDebt;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using Voltix.Infrastructure.SignlR;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Voltix.Application.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IPaymentService _paymentService;
        private readonly IDealerConfigurationService _depositSetting;
        private readonly IDealerDebtService _dealerDebtService;
        private readonly IDealerDebtTransactionService _dealerDebtTransactionService;
        private readonly IEContractService _eContractService;
        private readonly ILogService _logService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public CustomerOrderService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService, IDealerConfigurationService depositSetting,
            IDealerDebtService dealerDebtService, IDealerDebtTransactionService dealerDebtTransactionService, IEContractService eContractService, ILogService logService,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _paymentService = paymentService;
            _depositSetting = depositSetting;
            _dealerDebtService = dealerDebtService;
            _dealerDebtTransactionService = dealerDebtTransactionService;
            _eContractService = eContractService;
            _logService = logService;
            _hubContext = hubContext;
        }

        public async Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateCustomerOrderDTO createCustomerOrderDTO, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found.",
                        StatusCode = 404,
                    };
                }

                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(createCustomerOrderDTO.QuoteId);
                if (quote == null || quote.DealerId != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found.",
                        StatusCode = 404,
                    };
                }

                if (quote.Status != QuoteStatus.Accepted)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote is not accepted yet. Cann't create order.",
                        StatusCode = 400,
                    };
                }

                var orderNo = _unitOfWork.CustomerOrderRepository.GenerateOrderNumber();

                OrderStatus status = OrderStatus.ConfirmPending;
                var amount = quote.TotalAmount;
                decimal? deposit = null;

                if (!createCustomerOrderDTO.IsPayFull)
                {
                    var depositRate = await _depositSetting.GetCurrentConfigurationAsync(user, ct);
                    deposit = amount * (depositRate.Data!.MaxDepositPercentage / 100);
                }

                var customerOrder = new CustomerOrder
                {
                    CustomerId = createCustomerOrderDTO.CustomerId,
                    QuoteId = quote.Id,
                    OrderNo = orderNo,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = amount,
                    DepositAmount = deposit.HasValue ? (int)deposit.Value : (int?)null,
                    Status = status,
                    CreatedBy = userId,
                    Quote = quote
                };

                await _unitOfWork.CustomerOrderRepository.AddAsync(customerOrder, ct);

                await HandleOrderDetail(customerOrder, ct);
                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(user, LogType.Create, "Customer", customerOrder.OrderNo.ToString(), CancellationToken.None);
                var getCustomerOrder = _mapper.Map<GetCustomerOrderDTO>(customerOrder);

                if (createCustomerOrderDTO.IsPayFull)
                {
                    await _eContractService.CreatePayFullConfirmationEContract(customerOrder.Id, ct);
                }
                else
                {
                    await _eContractService.CreateDepositEContractConfirm(customerOrder.Id, ct);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create customer order successfully.",
                    StatusCode = 201,
                    Result = getCustomerOrder,
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> CustomerConfirm(Guid customerOrderId, string email, bool isAccept, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (customerOrder.Customer.Email != email)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Email does not match with the customer order.",
                        StatusCode = 400,
                    };
                }

                var status = customerOrder.Status;
                if (status != OrderStatus.ConfirmPending && status != OrderStatus.RemainingPending)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only orders with ConfirmPending or RemainingPending status can be confirmed by customer.",
                        StatusCode = 400,
                    };
                }

                if (isAccept && customerOrder.Status.Equals(OrderStatus.ConfirmPending))
                {
                    status = OrderStatus.Confirmed;
                }
                else if (isAccept && customerOrder.Status.Equals(OrderStatus.RemainingPending))
                {
                    status = OrderStatus.RemainingConfimmed;
                }
                else
                {
                    var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllByCustomerOrderId(customerOrderId, ct);
                    {
                        if (orderDetails != null && orderDetails.Count > 0)
                        {
                            await RestoreVehicleStatus(orderDetails);
                        }
                    }
                    status = OrderStatus.Rejected;
                }

                customerOrder.Status = status;
                _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                await _unitOfWork.SaveAsync();
                await UpdateStatusRealTime(customerOrder.Quote.DealerId);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Customer order confirmed successfully.",
                    StatusCode = 200,
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to confirm customer order: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> PayCustomerOrder(ClaimsPrincipal userClaim, ConfirmCustomerOrderDTO confirmCustomerOrderDTO, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found.",
                        StatusCode = 404,
                    };
                }

                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(confirmCustomerOrderDTO.CustomerOrderId);

                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (customerOrder.Status != OrderStatus.Confirmed && customerOrder.Status != OrderStatus.RemainingConfimmed)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only orders with Confirmed or RemainingConfimmed status can confirm cash receipt.",
                        StatusCode = 400,
                    };
                }

                var originalStatus = customerOrder.Status;

                decimal paidAmount;
                string paymentNote;
                string eContractNote;

                if (confirmCustomerOrderDTO.IsCash)
                {
                    if (confirmCustomerOrderDTO.IsPayFull)
                    {
                        paidAmount = customerOrder.TotalAmount;
                        paymentNote = $"Full cash payment for order {customerOrder.OrderNo}";
                        eContractNote = "FullPayment";

                        var transaction = new Transaction
                        {
                            Amount = paidAmount,
                            CustomerOrderId = customerOrder.Id,
                            Status = TransactionStatus.Success,
                            OrderRef = customerOrder.OrderNo.ToString(),
                            Currency = "VND",
                            Note = $"Full payment for order {customerOrder.OrderNo}",
                            Provider = "Cash",
                        };
                        await _unitOfWork.TransactionRepository.AddAsync(transaction, ct);

                        var recordPayments = new RecordPaymentDTO
                        {
                            PaidAtUtc = DateTime.UtcNow,
                            Amount = paidAmount,
                            ReferenceNo = $"CustomerOrderId|{customerOrder.OrderNo}",
                            Note = $"Pay full payment for order {customerOrder.OrderNo}",
                            Method = "Cash",
                        };
                        await _dealerDebtService.AddPaymentForDealerAsync(dealer.Id, recordPayments, ct);

                        customerOrder.Status = OrderStatus.Completed;
                    }
                    else if (!confirmCustomerOrderDTO.IsPayFull)
                    {
                        if (customerOrder.DepositAmount is null)
                        {
                            return new ResponseDTO
                            {
                                IsSuccess = false,
                                Message = "Deposit amount is not set for this order.",
                                StatusCode = 400,
                            };
                        }

                        paidAmount = customerOrder.DepositAmount.Value;
                        paymentNote = $"Cash deposit payment for order {customerOrder.OrderNo}";
                        eContractNote = "Deposit";

                        var transaction = new Transaction
                        {
                            Amount = paidAmount,
                            CustomerOrderId = customerOrder.Id,
                            Status = TransactionStatus.Success,
                            OrderRef = customerOrder.OrderNo.ToString(),
                            Currency = "VND",
                            Note = $"Deposit for order {customerOrder.OrderNo}",
                            Provider = "Cash",
                        };
                        await _unitOfWork.TransactionRepository.AddAsync(transaction, ct);

                        var recordPayments = new RecordPaymentDTO
                        {
                            PaidAtUtc = DateTime.UtcNow,
                            Amount = paidAmount,
                            ReferenceNo = $"CustomerOrderId|{customerOrder.OrderNo}",
                            Note = $"Deposit payment for order {customerOrder.OrderNo}",
                            Method = "Cash",
                        };
                        await _dealerDebtService.AddPaymentForDealerAsync(dealer.Id, recordPayments, ct);
                        customerOrder.Status = OrderStatus.Depositing;
                    }
                    else
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Invalid order status for confirming cash receipt.",
                            StatusCode = 400,
                        };
                    }
                    _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                }
                else
                {
                    var payemntOnline = await _paymentService.CreateVNPayLink(customerOrder.Id, ct);
                }

                await _unitOfWork.SaveAsync();

                await UpdateStatusRealTime(dealer.Id);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Cash receipt confirmed successfully.",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to confirm cash receipt: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }


        private async Task HandleOrderDetail(CustomerOrder customerOrder, CancellationToken ct)
        {
            var orderDetails = customerOrder.Quote.QuoteDetails;
            foreach (var quoteDetail in orderDetails)
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetVehicleByQuantityWithOldestImportDateForDealerAsync(
                        quoteDetail.VersionId,
                        quoteDetail.ColorId,
                        customerOrder.Quote.Dealer.Warehouse.Id,
                        quoteDetail.Quantity);

                foreach (var vehicle in vehicles)
                {
                    var orderDetail = new OrderDetail
                    {
                        CustomerOrderId = customerOrder.Id,
                        ElectricVehicleId = vehicle.Id
                    };
                    await _unitOfWork.OrderDetailRepository.AddAsync(orderDetail, ct);
                    if (customerOrder.Status is OrderStatus.Completed)
                    {
                        vehicle.Status = ElectricVehicleStatus.Sold;
                    }
                    else if (customerOrder.Status is OrderStatus.Depositing)
                    {
                        vehicle.Status = ElectricVehicleStatus.DepositBooked;
                    }
                    else if (customerOrder.Status is OrderStatus.ConfirmPending)
                    {
                        vehicle.Status = ElectricVehicleStatus.DealerPending;
                    }
                    else
                    {
                        throw new Exception("Invalid order status for handling order detail.");
                    }
                    _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                }
            }
        }

        public async Task<ResponseDTO> GetAllCustomerOrders(ClaimsPrincipal userClaim, int pageNumber, int pageSize, OrderStatus? orderStatus, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer is null)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found.",
                            StatusCode = 404,
                        };
                    }
                }

                Func<IQueryable<CustomerOrder>, IQueryable<CustomerOrder>> includes = co => co
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.ElectricVehicleVersion)
                                .ThenInclude(v => v.Model)
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.ElectricVehicleColor)
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.Promotion)
                            .Include(co => co.Customer)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.ElectricVehicleTemplate)
                                .ThenInclude(ev => ev.Color)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.ElectricVehicleTemplate)
                                .ThenInclude(t => t.Version)
                                .ThenInclude(v => v.Model)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.Warehouse)
                            .Include(co => co.EContracts);

                Expression<Func<CustomerOrder, bool>> filter = co => co.Quote.DealerId == dealer.Id;

                (IReadOnlyList<CustomerOrder> items, int total) result;
                result = await _unitOfWork.CustomerOrderRepository.GetPagedAsync(
                            filter: filter,
                            includes: includes,
                            orderBy: dm => dm.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var getOrderList = _mapper.Map<List<GetCustomerOrderDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Customer orders retrieved successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        data = getOrderList,
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
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to get all customer order: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> CancelCustomerOrderAsync(Guid customerOrderId, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (customerOrder.Status == OrderStatus.Completed || customerOrder.Status == OrderStatus.Cancelled)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Completed or cancelled orders cannot be cancelled.",
                        StatusCode = 400,
                    };
                }

                customerOrder.Status = OrderStatus.Cancelled;
                _unitOfWork.CustomerOrderRepository.Update(customerOrder);

                var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllByCustomerOrderId(customerOrderId, ct);
                {
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        await RestoreVehicleStatus(orderDetails);
                    }
                }

                await _unitOfWork.SaveAsync();
                await _logService.AddLogAsync(null, LogType.Update, "CustomerOrder", customerOrder.OrderNo.ToString(), ct);

                await UpdateStatusRealTime(customerOrder.Quote.DealerId);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Customer order cancelled successfully.",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to cancel: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        private async Task RestoreVehicleStatus(List<OrderDetail> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(orderDetail.ElectricVehicleId);
                if (vehicle != null)
                {
                    vehicle.Status = ElectricVehicleStatus.AtDealer;
                    _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                }
            }
        }

        public async Task<ResponseDTO> PayDeposit(Guid customerOrderId, bool? isCash, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (!customerOrder.Status.Equals(OrderStatus.RemainingConfimmed) && !customerOrder.Status.Equals(OrderStatus.Depositing) && !customerOrder.Status.Equals(OrderStatus.RemainingPending))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only orders with RemainingConfimmed or Depositing or RemainingPending status can pay deposit.",
                        StatusCode = 400,
                    };
                }

                if (isCash is not null && isCash.Value)
                {
                    customerOrder.Status = OrderStatus.Completed;
                    _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                    await HandleOrderDetail(customerOrder, ct);

                    var amount = customerOrder.TotalAmount - customerOrder.DepositAmount;
                    var transaction = new Transaction
                    {
                        Amount = amount!.Value,
                        CustomerOrderId = customerOrder.Id,
                        Status = TransactionStatus.Success,
                        OrderRef = customerOrder.OrderNo.ToString(),
                        Currency = "VND",
                        Note = $"Pay remain deposit for order {customerOrder.OrderNo}",
                        Provider = "Cash",
                    };
                    await _unitOfWork.SaveAsync();
                    await UpdateStatusRealTime(customerOrder.Quote.DealerId);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Deposit paid successfully with cash.",
                        StatusCode = 200,
                    };
                }
                else if (isCash is not null && !isCash.Value)
                {
                    var link = await _paymentService.CreateVNPayLink(customerOrder.Id, ct);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "VNPay link for deposit payment created successfully.",
                        StatusCode = 200,
                        Result = link
                    };
                }
                else if (isCash is null)
                {
                    customerOrder.Status = OrderStatus.RemainingPending;
                    _unitOfWork.CustomerOrderRepository.Update(customerOrder);
                    await _eContractService.CreatePayFullConfirmationEContract(customerOrder.Id, ct);
                    await _unitOfWork.SaveAsync(ct);
                    await UpdateStatusRealTime(customerOrder.Quote.DealerId);
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "E-Contract for full payment created successfully.",
                        StatusCode = 200,
                    };
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Invalid payment method.",
                        StatusCode = 400,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to pay deposit: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> AutoCancelExpiredDepositOrders(CancellationToken ct)
        {
            try
            {
                var dealerConfigs = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                if (dealerConfigs is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "Default dealer configuration not found.",
                    };
                }
                int dayCancel = dealerConfigs.DayCancelDeposit;     
                const decimal PLATFORM_PERCENT = 0.3m;     
                const decimal DEALER_PERCENT = 0.7m;     

                var nowUtc = DateTime.UtcNow;
                var depositOrders = await _unitOfWork.CustomerOrderRepository.GetAllCustomerOrderDeposit(ct);

                if (depositOrders == null || !depositOrders.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "No depositing orders to check.",
                    };
                }

                int checkedCount = 0;
                int cancelledCount = 0;

                var affectedDealerIds = new HashSet<Guid>();

                foreach (var order in depositOrders)
                {
                    checkedCount++;
                    var cancelDays = dayCancel;

                    var expiredAt = order.CreatedAt.AddDays(cancelDays);

                    if (nowUtc <= expiredAt)
                        continue;

                    var depositAmount = order.DepositAmount ?? 0m;
                    if (depositAmount <= 0)
                        continue;

                    var platformShare = Math.Round(depositAmount * PLATFORM_PERCENT, 0);
                    var dealerShare = depositAmount - platformShare;

                    order.Status = OrderStatus.Cancelled;
                    _unitOfWork.CustomerOrderRepository.Update(order);

                    var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllByCustomerOrderId(order.Id, ct);
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        await RestoreVehicleStatus(orderDetails);
                    }

                    cancelledCount++;
                    affectedDealerIds.Add(order.Quote.DealerId);
                }

                await _unitOfWork.SaveAsync();

                foreach (var dealerId in affectedDealerIds)
                {
                    await UpdateStatusRealTime(dealerId);
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = $"Cron checked {checkedCount} deposit orders, cancelled {cancelledCount} expired orders.",
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error when cancelling expired deposit orders: {ex.Message}",
                };
            }
        }

        public async Task<ResponseDTO> AutoCancelExpiredPendingOrders(CancellationToken ct)
        {
            try
            {
                var dealerConfigs = await _unitOfWork.DealerConfigurationRepository.GetByDefaultAsync(ct);
                if (dealerConfigs is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "Default dealer configuration not found."
                    };
                }

                int dayCancel = dealerConfigs.DayCancelDeposit;

                var nowUtc = DateTime.UtcNow;

                var pendingOrders = await _unitOfWork.CustomerOrderRepository.GetAllCustomerOrderPending(ct);

                if (pendingOrders == null || !pendingOrders.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "No pending orders to check."
                    };
                }

                int checkedCount = 0;
                int cancelledCount = 0;

                var affectedDealerIds = new HashSet<Guid>();

                foreach (var order in pendingOrders)
                {
                    checkedCount++;

                    var expiredAt = order.CreatedAt.AddDays(dayCancel);

                    if (nowUtc <= expiredAt)
                        continue;

                    order.Status = OrderStatus.Cancelled;
                    _unitOfWork.CustomerOrderRepository.Update(order);

                    var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllByCustomerOrderId(order.Id, ct);
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        await RestoreVehicleStatus(orderDetails);
                    }

                    cancelledCount++;
                    affectedDealerIds.Add(order.Quote.DealerId);
                }

                await _unitOfWork.SaveAsync();

                foreach (var dealerId in affectedDealerIds)
                {
                    await UpdateStatusRealTime(dealerId);
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = $"Cron checked {checkedCount} pending orders, cancelled {cancelledCount} expired orders."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error when cancelling expired pending orders: {ex.Message}"
                };
            }
        }



        private async Task<ResponseDTO> UpdateStatusRealTime(Guid dealerId)
        {
            var groupName = $"dealer:{dealerId}:all";

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveVehicleDeliveryStatusUpdate");

            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Real-time update delivery vehicle status sent successfully",
                StatusCode = 200
            };
        }
    }
}