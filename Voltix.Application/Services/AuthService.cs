using Amazon.Runtime.Internal.Util;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Voltix.Application.DTO.Auth;
using Voltix.Application.IService;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System.Security.Claims;

namespace Voltix.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        public AuthService(IUnitOfWork unitOfWork, IEmailService emailService, ITokenService tokenService, IMapper mapper, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<ResponseDTO> LoginUser(LoginUserDTO loginUserDTO, CancellationToken ct)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(loginUserDTO.Email);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not exist",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    var remainingMinutes = (user.LockoutEnd.Value - DateTimeOffset.UtcNow).Minutes;
                    return new ResponseDTO
                    {
                        Message = $"Account is locked. Try again in {remainingMinutes} minutes.",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }

                if (user.LockoutEnabled)
                {
                    return new ResponseDTO
                    {
                        Message = "Account is deactivated. Please contact support.",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }

                var isPasswordValid = await _unitOfWork.UserManagerRepository.CheckPasswordAsync(user, loginUserDTO.Password);
                if (!isPasswordValid)
                {
                    await _unitOfWork.UserManagerRepository.AccessFailedAsync(user);
                    return new ResponseDTO
                    {
                        Message = $"Password is incorrect. If you enter {5 - user.AccessFailedCount} incorrectly again, your account will be locked for 5 minutes.",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                if (!user.EmailConfirmed)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not verified",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var accessToken = await _tokenService.GenerateJwtAccessTokenAysnc(user, ct);
                var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user, loginUserDTO.RememberMe);

                var getUser = _mapper.Map<GetApplicationUserDTO>(user);

                await _unitOfWork.UserManagerRepository.ResetAccessFailedAsync(user);

                return new ResponseDTO
                {
                    Message = "Login successful",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserData = getUser
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at LoginUser in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(forgotPasswordDTO.Email);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "Email is not exist",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var token = await _unitOfWork.UserManagerRepository.GeneratePasswordResetTokenAsync(user);

                var encodedToken = Uri.EscapeDataString(token);
                var resetLink = $"{StaticLinkUrl.WebUrl}/api/reset-password?userId={user.Id}&token={encodedToken}";

                var isSendSuccess = await _emailService.SendResetPassword(user.Email, resetLink);
                if (!isSendSuccess)
                {
                    return new ResponseDTO
                    {
                        Message = "Failed to send reset password email",
                        IsSuccess = false,
                        StatusCode = 500
                    };
                }

                return new ResponseDTO
                {
                    Message = "Reset password email sent successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ForgotPassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(resetPasswordDTO.UserId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var decodedToken = Uri.UnescapeDataString(resetPasswordDTO.Token);
                var isSuccess = await _unitOfWork.UserManagerRepository.ResetPasswordAsync(user, decodedToken, resetPasswordDTO.Password);

                if (!isSuccess.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Password reset failed, Token not correct",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                return new ResponseDTO
                {
                    Message = "Password reset successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ResetPassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> ChangePassword(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var user = await _unitOfWork.UserManagerRepository.GetByIdAsync(userId);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        Message = "User not found",
                        IsSuccess = false,
                        StatusCode = 404
                    };
                }

                var isOldPasswordValid = await _unitOfWork.UserManagerRepository.CheckPasswordAsync(user, changePasswordDTO.CurrentPassword);
                if (!isOldPasswordValid)
                {
                    return new ResponseDTO
                    {
                        Message = "Current password is incorrect",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var isSuccess = await _unitOfWork.UserManagerRepository.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
                if (!isSuccess.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Message = "Change password failed",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                return new ResponseDTO
                {
                    Message = "Change password successfully",
                    IsSuccess = true,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Message = $"An error occurred at ChangePassword in AuthService: {ex.Message}",
                    IsSuccess = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> HandleGoogleCallbackAsync(ClaimsPrincipal userClaims, CancellationToken ct)
        {
            try
            {
                var email = userClaims.FindFirst(ClaimTypes.Email)?.Value;
                var name = userClaims.FindFirst(ClaimTypes.Name)?.Value;
                var googleSub = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (googleSub is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = "Google Sub (NameIdentifier) claim is missing."
                    };
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "User not found in internal system."
                    };
                }

                var user = await _unitOfWork.UserManagerRepository.GetByEmailAsync(email);

                if (user is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "User not found"
                    };
                }
                var logins = await _unitOfWork.UserManagerRepository.HasLogin(user);
                var hasGoogleLinked = logins.Any(login => login.LoginProvider == "Google" && login.ProviderKey == googleSub);
                if (!hasGoogleLinked)
                {
                    var linkResult = await _unitOfWork.UserManagerRepository.AddLoginGoogleAsync(user, googleSub);
                    if (!linkResult.Succeeded)
                    {
                        var msg = string.Join("; ", linkResult.Errors.Select(e => e.Description));
                        var status = msg.Contains("already exists", StringComparison.OrdinalIgnoreCase) ? 409 : 500;
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = status,
                            Message = $"Failed to link Google account: {msg}"
                        };
                    }
                }

                var accessToken = await _tokenService.GenerateJwtAccessTokenAysnc(user, ct);
                var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user, rememberMe: true);

                await _unitOfWork.SaveAsync();
                var getUser = _mapper.Map<GetApplicationUserDTO>(user);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Google login successful",
                    Result = new AuthResultDTO
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserData = getUser
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error at HandleGoogleCallbackAsync: {ex.Message}"
                };
            }
        }

        public Task StoreAsync(string ticket, AuthResultDTO value, TimeSpan ttl)
        {
            _cache.Set(ticket, value, ttl);
            return Task.CompletedTask;
        }

        public Task<AuthResultDTO?> RedeemAsync(string ticket)
        {
            if (_cache.TryGetValue<AuthResultDTO>(ticket, out var payload))
            {
                _cache.Remove(ticket);
                return Task.FromResult<AuthResultDTO?>(payload);
            }
            return Task.FromResult<AuthResultDTO?>(null);
        }

        public async Task<ResponseDTO> RegisterMobile(RegisterMobileDTO registerMobileDTO)
        {
            try
            {
                if (registerMobileDTO.Email is not null)
                {
                    var isExistEmail = await _unitOfWork.UserManagerRepository.IsEmailExist(registerMobileDTO.Email);
                    if (isExistEmail)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            StatusCode = 409,
                            Message = $"{registerMobileDTO.Email} existed"
                        };
                    }
                }

                var isExistUserName = await _unitOfWork.UserManagerRepository.IsExistUserName(registerMobileDTO.UserName);
                if (isExistUserName)
                  {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 409,
                        Message = "User name is exist"
                    };
                  }

                var newUser = new ApplicationUser
                {
                    UserName = registerMobileDTO.UserName,
                    Email = registerMobileDTO.Email,
                    PhoneNumber = registerMobileDTO.PhoneNumber,
                    Address = registerMobileDTO.Address
                };

                var created = await _unitOfWork.UserManagerRepository.CreateAsync(newUser, registerMobileDTO.Password);
                if (!created.Succeeded)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "Cannot create new account",
                        Result = created
                    };
                }

                newUser.LockoutEnabled = false;
                _unitOfWork.UserManagerRepository.Update(newUser);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    StatusCode = 201,
                    Message = "Create new user successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to register a new account {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> LoginMobile(LoginMobileDTO loginMobileDTO, CancellationToken ct)
        {
            try
            {
                var user = await _unitOfWork.UserManagerRepository.GetByUserNameAsync(loginMobileDTO.Username);
                if (user is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = "The user is not exist"
                    };
                }

                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    var remainingMinutes = (user.LockoutEnd.Value - DateTimeOffset.UtcNow).Minutes;
                    return new ResponseDTO
                    {
                        Message = $"Account is locked. Try again in {remainingMinutes} minutes.",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }

                if (user.LockoutEnabled)
                {
                    return new ResponseDTO
                    {
                        Message = "Account is deactivated. Please contact support.",
                        IsSuccess = false,
                        StatusCode = 403
                    };
                }
                var isPasswordValid = await _unitOfWork.UserManagerRepository.CheckPasswordAsync(user, loginMobileDTO.Password);
                if (!isPasswordValid)
                {
                    await _unitOfWork.UserManagerRepository.AccessFailedAsync(user);
                    return new ResponseDTO
                    {
                        Message = $"Password is incorrect. If you enter {5 - user.AccessFailedCount} incorrectly again, your account will be locked for 5 minutes.",
                        IsSuccess = false,
                        StatusCode = 400
                    };
                }

                var accessToken = await _tokenService.GenerateJwtAccessTokenAysnc(user, ct);
                var refreshToken = await _tokenService.GenerateJwtRefreshTokenAsync(user, true);

                var getUser = _mapper.Map<GetApplicationUserDTO>(user);

                await _unitOfWork.UserManagerRepository.ResetAccessFailedAsync(user);

                return new ResponseDTO
                {
                    Message = "Login successful",
                    IsSuccess = true,
                    StatusCode = 200,
                    Result = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserData = getUser
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Message = $"Error to login mobile {ex.Message}"
                };
            }
        }
    }
}
