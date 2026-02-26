using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Voltix.Application.IService;
using Voltix.Application.IServices;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Voltix.Application.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        public TokenService(IConfiguration configuration, IUnitOfWork unitOfWork, IRedisService redisService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        }
        public async Task<string> GenerateJwtAccessTokenAysnc(ApplicationUser user, CancellationToken ct)
        {
            var roles = await _unitOfWork.UserManagerRepository.GetRoleAsync(user);

            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty),
            };

            var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerIdAsync(user.Id, ct);
            if (dealer is null)
            {
                dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(user.Id, ct);
            }

            if (dealer is not null)
            {
                authClaims.Add(new Claim("DealerId", dealer.Id.ToString()));
                authClaims.Add(new Claim("DealerName", dealer.Name));
            }

            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty));
            var authSigningCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

            var expires = _configuration.GetValue<int>("JWT:AccessTokenExpiration");
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(expires),
                claims: authClaims,
                signingCredentials: authSigningCredentials
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return accessToken;
        }

        public async Task<string> GenerateJwtRefreshTokenAsync(ApplicationUser user, bool rememberMe)
        {
            var authClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? string.Empty));
            var authSigningCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

            var expiration = GetRefreshTokenExpiration(rememberMe);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.Add(expiration),
                claims: authClaim,
                signingCredentials: authSigningCredentials
                );

            var refreshToken = new JwtSecurityTokenHandler().WriteToken(token);
            await StoreRefreshToken(user.Id, refreshToken, expiration);

            return refreshToken;
        }

        private TimeSpan GetRefreshTokenExpiration(bool rememberMe)
        {
            var rememberMeExpiration = _configuration.GetValue<int>("JWT:RefreshTokenExpiration:RememberMe");
            var normalExpiration = _configuration.GetValue<int>("JWT:RefreshTokenExpiration:Normal");

            return rememberMe ? TimeSpan.FromDays(rememberMeExpiration) : TimeSpan.FromHours(normalExpiration);
        }

        private async Task<bool> StoreRefreshToken(string userId, string refreshToken, TimeSpan expiration)
        {
            string redisKey = $"{StaticRedisKey.RefreshToken}:{userId}";
            var result = await _redisService.StoreKeyAsync(redisKey, refreshToken, expiration);

            return result;
        }
    }
}
