using AutoMapper;
using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVC;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.Services
{
    public class EVCService : IEVCService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        public EVCService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> CreateEVMStaff(CreateEVMStaffDTO createEVMStaffDTO)
        {
            try
            {
                var isEmailExist = await _unitOfWork.UserManagerRepository.IsEmailExist(createEVMStaffDTO.Email);
                if (isEmailExist)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "Email already exists."
                    };
                }

                var user = new ApplicationUser
                {
                    Email = createEVMStaffDTO.Email,
                    UserName = createEVMStaffDTO.Email,
                    FullName = createEVMStaffDTO.FullName,
                    PhoneNumber = createEVMStaffDTO.PhoneNumber,
                    EmailConfirmed = true
                };

                var password = "EVMStaff@" + Guid.NewGuid().ToString()[..6].ToUpper();
                var result = await _unitOfWork.UserManagerRepository.CreateAsync(user, password);

                user.LockoutEnabled = false;
                _unitOfWork.UserManagerRepository.Update(user);
                if (!result.Succeeded)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        StatusCode = 500,
                        Message = "Failed to create EVM Staff."
                    };
                }

                await _unitOfWork.UserManagerRepository.AddToRoleAsync(user, StaticUserRole.EVMStaff);
                await _unitOfWork.SaveAsync();

                await _emailService.SendEVMStaffAccountEmail(createEVMStaffDTO.Email, createEVMStaffDTO.FullName, password);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "EVM Staff created successfully."
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

        public async Task<ResponseDTO> GetAllEVMStaff(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int pageSize)
        {
            try
            {
                IEnumerable<ApplicationUser> evmStaffs = (await _unitOfWork.UserManagerRepository.GetUsersInRoleAsync(StaticUserRole.EVMStaff))
                    .OrderBy(s => s.LockoutEnabled);
                if (evmStaffs is null || !evmStaffs.Any())
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "No EVM Staff found."
                    };
                }

                if (!string.IsNullOrEmpty(filterOn) && !string.IsNullOrEmpty(filterQuery))
                {
                    evmStaffs = filterOn.ToLower().Trim() switch
                    {
                        "email" => evmStaffs.Where(u => u.Email != null && u.Email.Contains(filterQuery, StringComparison.OrdinalIgnoreCase)),
                        "fullname" => evmStaffs.Where(u => u.FullName != null && u.FullName.Contains(filterQuery, StringComparison.OrdinalIgnoreCase)),
                        "phonenumber" => evmStaffs.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(filterQuery, StringComparison.OrdinalIgnoreCase)),

                        _ => evmStaffs
                    };
                }

                if (pageNumber > 0 && pageSize > 0)
                {
                    evmStaffs = evmStaffs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Invalid page number or page size."
                    };
                }

                var result = _mapper.Map<List<GetApplicationUserDTO>>(evmStaffs);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "EVM Staffs retrieved successfully.",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to get all EVM staffs: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateEVCStaffStatus(string evcStaffId, bool isActive, CancellationToken ct)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(evcStaffId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "EVC Staff not found."
                    };
                }

                if (isActive)
                {
                    user.LockoutEnabled = false;
                }
                else
                {
                    user.LockoutEnabled = true;
                }

                _unitOfWork.UserManagerRepository.Update(user);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "EVC Staff status updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to update EVC staff status: {ex.Message}"
                };
            }
        }
    }
}
