using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IBookingEVRepository : IRepository<BookingEV>
    {
        Task<BookingEV?> GetBookingWithIdAsync(Guid bookingId);
        Task<bool> IsBookingExistsById(Guid bookingId);
        Task<List<BookingEV>> GetAllBookingWithDetailAsync();
        Task<List<ElectricVehicle?>> GetVehiclesByBookingIdAsync(Guid bookingId);
        Task<int> CountByDealerIdAsync(Guid dealerId, CancellationToken ct);
        Task<int> GetTotalBookingsAsync(CancellationToken ct);
    }
}
