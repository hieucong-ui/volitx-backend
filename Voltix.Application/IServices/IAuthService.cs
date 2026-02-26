using Voltix.Application.DTO.Auth;
using Voltix.Domain.Entities;
using System.Security.Claims;
namespace Voltix.Application.IService
{
    public interface IAuthService
    {
        Task<ResponseDTO> LoginUser(LoginUserDTO loginUserDTO, CancellationToken ct);
        Task<ResponseDTO> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO);
        Task<ResponseDTO> ResetPassword(ResetPasswordDTO resetPasswordDTO);
        Task<ResponseDTO> ChangePassword(ChangePasswordDTO changePasswordDTO, ClaimsPrincipal userClaims);
        Task<ResponseDTO> HandleGoogleCallbackAsync(ClaimsPrincipal userClaims, CancellationToken ct);
        Task StoreAsync(string ticket, AuthResultDTO value, TimeSpan ttl);
        Task<AuthResultDTO?> RedeemAsync(string ticket);
        Task<ResponseDTO> RegisterMobile(RegisterMobileDTO registerMobileDTO);
        Task<ResponseDTO> LoginMobile(LoginMobileDTO loginMobileDTO, CancellationToken ct);
    }
}
