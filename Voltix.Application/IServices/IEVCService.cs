using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.EVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IEVCService
    {
        Task<ResponseDTO> CreateEVMStaff(CreateEVMStaffDTO createEVMStaffDTO);
        Task<ResponseDTO> GetAllEVMStaff(string? filterOn, string? filterQuery, string? sortBy, bool? isAcsending, int pageNumber, int pageSize);
        Task<ResponseDTO> UpdateEVCStaffStatus(string evcStaffId, bool isActive, CancellationToken ct);
    }
}
