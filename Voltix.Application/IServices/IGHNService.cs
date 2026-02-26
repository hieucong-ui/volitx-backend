using Voltix.Application.DTO.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IGHNService
    {
        Task<ResponseDTO> GetProvincesAsync();
        Task<ResponseDTO> GetDistrictsAsync(int provinceId);
        Task<ResponseDTO> GetWardsAsync(int districtId);
        Task<ResponseDTO> ProvincesOpenGetProvinceResponse(CancellationToken ct);
        Task<ResponseDTO> ProvincesOpenGetWardResponse(string provinceCode, CancellationToken ct);
    }
}
