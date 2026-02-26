using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dashboard;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class DashBoardService : IDashBoardService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public DashBoardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseDTO> GetDealerDashboardAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(user == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 401,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId,ct);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var dealerId = dealer.Id;

                var totalBookings = await _unitOfWork.BookingEVRepository.CountByDealerIdAsync(dealerId, ct);
                var totalDelivery = await _unitOfWork.VehicleDeliveryRepository.CountByDealerIdAsync(dealerId, ct);
                var totalQuote = await _unitOfWork.QuoteRepository.CountByDealerIdAsync(dealerId, ct);
                var totalVehicle = await _unitOfWork.VehicleDeliveryRepository.CountByDealerIdAsync(dealerId, ct);
                var totalCustomer = await _unitOfWork.CustomerRepository.CountCustomerByDealerId(dealerId, ct);
                var totalActiveStaff = await _unitOfWork.DealerMemberRepository.TotalDealerMember(dealerId, ct);

                var dashboard = new GetDealerManagerDBDTO
                {
                    DealerName = dealer.Name,
                    TotalBookings = totalBookings,
                    TotalDeliveries = totalDelivery,
                    TotalQuotes = totalQuote,
                    TotalVehicles = totalVehicle,
                    TotalCustomers = totalCustomer,
                    TotalActiveStaff = totalActiveStaff
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get Dealer Dashboard Successfully ",
                    StatusCode = 200,
                    Result = dashboard
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetDealerRevenueByQuarterAsync(ClaimsPrincipal user, int year, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 401
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId,ct);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var transactions = await _unitOfWork.TransactionRepository.GetDealerTransactionsByYearAsync(dealer.Id, year, ct);

                var revenueByQuarter = transactions
                   .GroupBy(t => new
                   {
                       Quarter = (t.CreatedAt.Month - 1) / 3 + 1,
                       Year = t.CreatedAt.Year
                   })
                   .Select(g => new GetDealerRevenueByQuarterDTO
                   {
                       Year = g.Key.Year,
                       Quarter = g.Key.Quarter,
                       TotalRevenue = g.Sum(x => x.Amount),
                       TotalSoldVehicles = g.SelectMany(x => x.CustomerOrder.OrderDetails).Count()
                   })
                   .OrderBy(r => r.Quarter)
                   .ToList();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get dealer revenue by quarter successfully",
                    StatusCode = 200,
                    Result = revenueByQuarter
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetDealerStaffDashboardAsync(ClaimsPrincipal user, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 401
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var totalQuote = await _unitOfWork.QuoteRepository.CountByDealerIdAsync(dealer.Id, ct);
                var totalVehicle = await _unitOfWork.VehicleDeliveryRepository.CountByDealerIdAsync(dealer.Id, ct);
                var totalCustomer = await _unitOfWork.CustomerRepository.CountCustomerByDealerId(dealer.Id, ct);

                var dashboard = new GetDealerStaffDBDTO
                {
                    DealerName = dealer.Name,
                    TotalQuotes = totalQuote,
                    TotalVehicles = totalVehicle,
                    TotalCustomers = totalCustomer,
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get dealer staff dashboard successfully",
                    StatusCode = 200,
                    Result = dashboard
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAdminDashboardAsync(CancellationToken ct)
        {
            try
            {
                var totalInventory = await _unitOfWork.EVCInventoryRepository.GetTotalEVCInventoryAsync(ct);
                var totalDealers = await _unitOfWork.DealerRepository.GetTotalDealersAsync(ct);
                var totalBookings = await _unitOfWork.BookingEVRepository.GetTotalBookingsAsync(ct);
                var totalDeliveries = await _unitOfWork.VehicleDeliveryRepository.GetTotalDeliveriesAsync(ct);
                var totalVehicles = await _unitOfWork.ElectricVehicleRepository.GetTotalVehiclesInEVCAsync(ct);
                var totalEVMStaff = await _unitOfWork.UserManagerRepository.GetTotalEVMStaffAsync(ct);

                var dashboard = new GetAdminDashboardDTO
                {
                    TotalEVCInventory = totalInventory,
                    TotalDealers = totalDealers,
                    TotalBookings = totalBookings,
                    TotalDeliveries = totalDeliveries,
                    TotalVehicles = totalVehicles,
                    TotalEVMStaff = totalEVMStaff
                };

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get admin dashboard successfully",
                    StatusCode = 200,
                    Result = dashboard
                };
            }
            catch(Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public Task<ResponseDTO> GetTotalCustomerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
