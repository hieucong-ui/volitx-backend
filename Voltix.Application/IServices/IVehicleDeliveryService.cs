using Voltix.Application.DTO.Auth;
using Voltix.Application.DTO.VehicleDelivery;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.IServices
{
    public interface IVehicleDeliveryService
    {
        Task<ResponseDTO> GetAllVehicleDelivery(ClaimsPrincipal user, int pageNumber, int pageSize, DeliveryStatus? status, Guid? templateId,bool isShow, CancellationToken ct);
        Task<ResponseDTO> GetVehicleDeliveryById(Guid deliveryId, CancellationToken ct);
        Task<ResponseDTO> UpdateVehicleDeliveryStatus(ClaimsPrincipal user, Guid deliveryId, DeliveryStatus newStatus, CancellationToken ct, string? reason = null);
        Task<ResponseDTO> InspectAccidentVehicleAsync(ClaimsPrincipal user, Guid deliveryId, List<Guid> damagedVehicleIds,bool isShow, CancellationToken ct);
        Task<ResponseDTO> ReplaceDamagedVehicleAsync(ClaimsPrincipal user, Guid deliveryId, CancellationToken ct);
    }
}