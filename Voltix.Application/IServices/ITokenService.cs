using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IService
{
    public interface ITokenService
    {
        Task<string> GenerateJwtAccessTokenAysnc(ApplicationUser user, CancellationToken ct);
        Task<string> GenerateJwtRefreshTokenAsync(ApplicationUser user, bool rememberMe);
    }
}
