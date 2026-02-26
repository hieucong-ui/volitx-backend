using Amazon.Runtime.Internal.UserAgent;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.Customer;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Voltix.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResponseDTO> CreateCustomerAsync(ClaimsPrincipal user, CreateCustomerDTO createCustomerDTO)
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
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetTrackedDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404,
                    };
                }

                var existPhone = dealer.Customers.FirstOrDefault(c => c.PhoneNumber == createCustomerDTO.PhoneNumber);
                if (existPhone != null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer phone number already exists in this dealer.",
                        StatusCode = 400,
                    };
                }

                var customerCurrent = await _unitOfWork.UserManagerRepository.GetByEmailAsync(createCustomerDTO.Email);
                Customer customer;
                if (customerCurrent is null)
                {
                    customer = new Customer
                    {
                        FullName = createCustomerDTO.FullName,
                        PhoneNumber = createCustomerDTO.PhoneNumber,
                        CitizenID = createCustomerDTO.CitizenID,
                        Email = createCustomerDTO.Email,
                        Address = createCustomerDTO.Address,
                        Note = createCustomerDTO.Note
                    };
                }
                else
                {
                    customer = new Customer
                    {
                        Id = Guid.Parse(customerCurrent.Id),
                        FullName = createCustomerDTO.FullName,
                        PhoneNumber = createCustomerDTO.PhoneNumber,
                        CitizenID = createCustomerDTO.CitizenID,
                        Email = createCustomerDTO.Email,
                        Address = createCustomerDTO.Address,
                        Note = createCustomerDTO.Note
                    };
                }

                customer.Dealers.Add(dealer);

                await _unitOfWork.CustomerRepository.AddAsync(customer, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var getCustomerDTO = _mapper.Map<GetCustomerDTO>(customer);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create customer successfully.",
                    StatusCode = 200,
                    Result = getCustomerDTO
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


        public async Task<ResponseDTO> GetAllCustomerAsync(ClaimsPrincipal user, int pageNumber, int pageSize, string? search, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not found. "
                    };
                }
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found."
                    };
                }

                Expression<Func<Customer, bool>> filter = c => c.Dealers.Any(d => d.Id == dealer.Id);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var loweredSearch = search.Trim().ToLower();
                    filter = c => c.Dealers.Any(d => d.Id == dealer.Id) &&
                                 (
                                     (c.FullName != null && c.FullName.Trim().ToLower().Contains(loweredSearch)) ||
                                     (c.Email != null && c.Email.Trim().ToLower().Contains(loweredSearch)) ||
                                     (c.PhoneNumber != null && c.PhoneNumber.Trim().ToLower().Contains(loweredSearch))
                                 );
                }
                Func<IQueryable<Customer>, IQueryable<Customer>>? includes = q => q
                    .Include(c => c.Dealers);

                (IReadOnlyList<Customer> items, int total) result =
                    await _unitOfWork.CustomerRepository.GetPagedAsync(
                        filter: filter,
                        includes: includes,
                        orderBy: c => c.CreatedAt,
                        ascending: true, // FIFO
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        ct: ct
                    );

                if (result.items == null || !result.items.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No customers found."
                    };
                }
                var getCustomers = _mapper.Map<List<GetCustomerDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Customers retrieved successfully",
                    Result = new
                    {
                        data = getCustomers,
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
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> GetCustomerByIdAsync(ClaimsPrincipal user, Guid customerId)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not found. "
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found."
                    };
                }

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);

                if (customer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer not found"
                    };
                }
                var getCustomer = _mapper.Map<GetCustomerDTO>(customer);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Customer retrieved successfully",
                    Result = getCustomer
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> UpdateCustomerAsync(ClaimsPrincipal user, Guid customerId, UpdateCustomerDTO updateCustomerDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 401,
                        Message = "User not found. "
                    };
                }
                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "Dealer not found."
                    };
                }

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Customer not found"
                    };
                }
                // check if customer belong to dealer
                var belongToDealer = dealer.Customers.Any(c => c.Id == customerId);
                if (!belongToDealer)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 403,
                        Message = "You are not allowed to update this customer."
                    };
                }

                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.PhoneNumber))
                {
                    var phone = updateCustomerDTO.PhoneNumber.Trim();
                    var existPhone = dealer.Customers
                        .FirstOrDefault(c => c.PhoneNumber == phone && c.Id != customer.Id);

                    if (existPhone != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This phone number already exists for another customer in your dealer."
                        };
                    }

                    customer.PhoneNumber = phone;
                }
                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.Email))
                {
                    var email = updateCustomerDTO.Email.Trim().ToLower();
                    var existEmail = dealer.Customers
                        .FirstOrDefault(c => c.Email != null && c.Email.ToLower() == email && c.Id != customer.Id);

                    if (existEmail != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This email already exists for another customer in your dealer."
                        };
                    }

                    customer.Email = email;
                }
                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.CitizenID))
                {
                    var citizenId = updateCustomerDTO.CitizenID.Trim();

                    // Optional: Check CCCD must have at least 9-12 digitals
                    if (!System.Text.RegularExpressions.Regex.IsMatch(citizenId, @"^\d{9,12}$"))
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Invalid CCCD format. Must be 9–12 digits."
                        };
                    }

                    var existCitizen = dealer.Customers
                        .FirstOrDefault(c => c.CitizenID == citizenId && c.Id != customer.Id);

                    if (existCitizen != null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "This CCCD already exists for another customer in your dealer."
                        };
                    }

                    customer.CitizenID = citizenId;
                }

                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.FullName))
                {
                    customer.FullName = updateCustomerDTO.FullName;
                }

                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.Address))
                {
                    customer.Address = updateCustomerDTO.Address;
                }

                if (!string.IsNullOrWhiteSpace(updateCustomerDTO.Note))
                {
                    customer.Note = updateCustomerDTO.Note;
                }

                _unitOfWork.CustomerRepository.Update(customer);
                await _unitOfWork.SaveAsync();

                var getCustomer = _mapper.Map<GetCustomerDTO>(customer);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Update customer successfully.",
                    Result = getCustomer
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = ex.Message
                };

            }

        }
    }
}
