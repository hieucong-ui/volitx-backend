using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Dealer;
using Voltix.Application.DTO.EContract;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Voltix.Application.Services
{
    public class DealerService : IDealerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly IDealerTierService _dealerTierService;
        public DealerService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, IDealerTierService dealerTierService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
            _dealerTierService = dealerTierService;
        }
        public async Task<ResponseDTO> CreateDealerStaffAsync(ClaimsPrincipal claimUser, CreateDealerStaffDTO createDealerStaffDTO, CancellationToken ct)
        {
            try
            {
                var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                var staff = await _unitOfWork.UserManagerRepository.GetByEmailAsync(createDealerStaffDTO.Email);
                ApplicationUser user;
                if (staff is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = createDealerStaffDTO.Email,
                        Email = createDealerStaffDTO.Email,
                        FullName = createDealerStaffDTO.FullName,
                        PhoneNumber = createDealerStaffDTO.PhoneNumber,
                    };

                    var randomPassword = "Staff@" + Guid.NewGuid().ToString()[..6].ToUpper();
                    var result = await _unitOfWork.UserManagerRepository.CreateAsync(user, randomPassword);

                    if (!result.Succeeded)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = "Failed to create Dealer Staff."
                        };
                    }

                    user.LockoutEnabled = false;
                    user.EmailConfirmed = true;
                    user.PhoneNumberConfirmed = true;

                    _unitOfWork.UserManagerRepository.Update(user);
                    await _unitOfWork.SaveAsync();

                    await _emailService.SendDealerStaffAaccountEmail(createDealerStaffDTO.Email, createDealerStaffDTO.FullName, randomPassword, dealer.Name);
                }
                else
                {
                    var isActiveDealerMember = await _unitOfWork.DealerMemberRepository.IsActiveDealerMemberByEmailAsync(dealer.Id, createDealerStaffDTO.Email, ct);
                    if (isActiveDealerMember)
                    {
                        var dealerActive = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(staff.Id, ct);
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = $"The user is still active at dealer {dealerActive!.Name}."
                        };
                    }

                    var isEmailExist = await _unitOfWork.DealerMemberRepository.IsExistDealerMemberByEmailAsync(dealer.Id, createDealerStaffDTO.Email, ct);
                    if (isEmailExist)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = "Email staff already exists."
                        };
                    }

                    user = staff;
                    await _emailService.NotifyAddedToDealerExistingUser(createDealerStaffDTO.Email, createDealerStaffDTO.FullName, $"Nhân viên đại lý", dealer.Name);
                    user.LockoutEnabled = false;
                    _unitOfWork.UserManagerRepository.Update(user);
                }

                await _unitOfWork.UserManagerRepository.AddToRoleAsync(user, StaticUserRole.DealerStaff);

                var dealerMember = new DealerMember
                {
                    ApplicationUserId = user.Id,
                    DealerId = dealer.Id,
                    RoleInDealer = DealerRole.Staff,
                };
                await _unitOfWork.DealerMemberRepository.AddAsync(dealerMember, ct);

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Dealer Staff created successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to create new dealer staff at DealerService:  {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetAllDealerAsync(string? filterOn, string? filterQuery, string? sortBy, DealerStatus? status, bool? isAcsending, int pageNumber, int PageSize, CancellationToken ct)
        {
            try
            {

                Expression<Func<Dealer, bool>>? baseFilter = null;
                if (status is not null)
                {
                    baseFilter = d => d.DealerStatus == status;
                }

                if (!string.IsNullOrWhiteSpace(filterOn) && (!string.IsNullOrWhiteSpace(filterQuery)))
                {
                    var query = filterQuery.Trim().ToLower();
                    baseFilter = filterOn.Trim().ToLower() switch
                    {
                        "name" => d => d.Name != null &&
                                       d.Name.ToLower().Contains(query),

                        _ => baseFilter
                    };
                }

                string sortField = (sortBy ?? "createdat").Trim().ToLower();
                //bool asc = isAcsending ?? true;

                (IReadOnlyList<Dealer> items, int total) result = (new List<Dealer>(), 0);
                Func<IQueryable<Dealer>, IQueryable<Dealer>> includes = q => q
                    .Include(dm => dm.Manager)
                    .Include(dm => dm.DealerTier);


                switch (sortField)
                {
                    case "name":
                        result = _unitOfWork.DealerRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: d => d.DealerStatus,
                            ascending: true,
                            pageNumber: pageNumber,
                            pageSize: PageSize,
                            ct: ct).Result;
                        break;

                    default:
                        result = _unitOfWork.DealerRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: d => d.DealerStatus,
                            ascending: true,
                            pageNumber: pageNumber,
                            pageSize: PageSize,
                            ct: ct).Result;
                        break;
                }

                var data = _mapper.Map<List<GetDealerDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get dealers successfully.",
                    Result = new
                    {
                        Data = data,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = PageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / PageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get all dealers at DealerService: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetAllDealerStaffAsync(ClaimsPrincipal claimUser, string? filterOn, string? filterQuery, string? sortBy, bool? isAscending, int pageNumber, int pageSize, CancellationToken ct)
        {
            try
            {
                var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet"
                    };
                }
                var dealer = _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct).Result;
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                Expression<Func<DealerMember, bool>> baseFilter = dm => dm.DealerId == dealer.Id;


                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    var q = filterQuery.Trim().ToLower();
                    baseFilter = filterOn.Trim().ToLower() switch
                    {
                        "fullname" => dm => dm.DealerId == dealer.Id &&
                                            dm.ApplicationUser.FullName != null &&
                                            dm.ApplicationUser.FullName.ToLower().Contains(q),
                        "email" => dm => dm.DealerId == dealer.Id &&
                                         dm.ApplicationUser.Email != null &&
                                         dm.ApplicationUser.Email.ToLower().Contains(q),
                        _ => dm => dm.DealerId == dealer.Id
                    };
                }

                Func<IQueryable<DealerMember>, IQueryable<DealerMember>> includes =
                    q => q.Include(dm => dm.ApplicationUser);

                string sortField = (sortBy ?? "createdat").Trim().ToLower();
                bool asc = isAscending ?? true;

                (IReadOnlyList<DealerMember> items, int total) result;

                switch (sortField)
                {
                    case "fullname":
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.FullName!,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;

                    case "email":
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.Email!,
                            ascending: asc,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;

                    default:
                        result = await _unitOfWork.DealerMemberRepository.GetPagedAsync(
                            filter: baseFilter,
                            includes: includes,
                            orderBy: dm => dm.ApplicationUser.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);
                        break;
                }

                var data = _mapper.Map<List<GetDealerStaffDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get dealer staffs successfully.",
                    Result = new
                    {
                        Data = data,
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
                    StatusCode = 500,
                    Message = $"Error to get all dealer staffs at DealerService:  {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> DealerInformationAsync(ClaimsPrincipal claimUser, CancellationToken ct)
        {
            try
            {
                var userId = claimUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not login yet"
                    };
                }
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                var memberTotal = await _unitOfWork.DealerMemberRepository.TotalDealerMember(dealer.Id, ct);
                var econtractDealer = await _unitOfWork.EContractRepository.GetEContractDealerByDealerIdAsync(userId, ct);
                if (econtractDealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer e-contract information not found"
                    };
                }

                var effectivePolicy = await _dealerTierService.GetEffectivePolicyAsync(dealer.Id, ct);
                if (effectivePolicy is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer tier policy information not found"
                    };
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Get dealer information successfully.",
                    Result = new
                    {
                        Dealer = _mapper.Map<GetDealerDTO>(dealer),
                        MemberTotal = memberTotal,
                        EcontractDealer = _mapper.Map<List<GetEContractDTO>>(econtractDealer),
                        EffectivePolicy = effectivePolicy
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get dealer information at DealerService:  {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateStatusDealer(Guid DealerId, DealerStatus status, CancellationToken ct)
        {
            try
            {
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(DealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer not found"
                    };
                }
                dealer.DealerStatus = status;
                _unitOfWork.DealerRepository.Update(dealer);

                if (status.Equals(DealerStatus.Inactive))
                {
                    dealer.Manager!.LockoutEnabled = true;
                    _unitOfWork.UserManagerRepository.Update(dealer.Manager);

                    foreach (var staff in dealer.DealerMembers)
                    {
                        staff.ApplicationUser.LockoutEnabled = true;
                        _unitOfWork.UserManagerRepository.Update(staff.ApplicationUser);
                    }
                }
                else if (status.Equals(DealerStatus.Active))
                {
                    dealer.Manager!.LockoutEnabled = false;
                    _unitOfWork.UserManagerRepository.Update(dealer.Manager);
                    foreach (var staff in dealer.DealerMembers)
                    {
                        if (staff.IsActive)
                        {
                            staff.ApplicationUser.LockoutEnabled = false;
                            _unitOfWork.UserManagerRepository.Update(staff.ApplicationUser);
                        }
                    }
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Dealer status updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error updating dealer status: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateStatusDealerStaff(ClaimsPrincipal userClaim, bool isActive, string applicationUserId, CancellationToken ct)
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
                        Message = "User not login yet"
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Users do not own any dealers"
                    };
                }

                var dealerMember = await _unitOfWork.DealerMemberRepository.IsDealerMemberBelongDealer(dealer.Id, applicationUserId, ct);
                if (!dealerMember)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer staff not found in your dealer"
                    };
                }

                var dealerStaff = await _unitOfWork.DealerMemberRepository.GetByApplicationId(applicationUserId, ct);
                if (dealerStaff is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Dealer staff not found"
                    };
                }

                dealerStaff.IsActive = isActive;

                if (isActive)
                {
                    dealerStaff.ApplicationUser.LockoutEnabled = false;
                }
                else
                {
                    dealerStaff.ApplicationUser.LockoutEnabled = true;
                }

                _unitOfWork.DealerMemberRepository.Update(dealerStaff);
                _unitOfWork.UserManagerRepository.Update(dealerStaff.ApplicationUser);

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Dealer staff status updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error updating dealer staff status: {ex.Message}"
                };
            }
        }
    }
}
