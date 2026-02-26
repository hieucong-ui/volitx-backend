using Voltix.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IClient
{
    public interface IGHNClient
    {
        Task<GhnResult<List<GhnProvince>>> GetProvincesAsync(CancellationToken ct = default);
        Task<GhnResult<List<GhnDistrict>>> GetDistrictsAsync(int provinceId, CancellationToken ct = default);
        Task<GhnResult<List<GhnWard>>> GetWardsAsync(int districtId, CancellationToken ct = default);
        Task<ProvincesOpenGetWardResponse> ProvincesOpenGetWardResponse(string provinceCode, CancellationToken ct = default);
        Task<List<ProvincesOpenGetProvinceResponse>> ProvincesOpenGetProvinceResponse(CancellationToken ct = default);
    }
}
