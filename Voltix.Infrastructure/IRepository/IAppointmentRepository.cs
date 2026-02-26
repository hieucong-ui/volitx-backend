using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<Appointment?> GetByIdAsync(Guid appointmentId);
        Task<List<Appointment>> GetByDealerIdAsync(Guid dealerId);
        Task<List<Appointment>> GetByCustomerIdAsync(Guid customerId);
        Task<List<Appointment>> GetAllByDealerIdAsync(Guid dealerId);
        Task<List<Appointment?>> GetByDealerIdAndDateAsync(Guid dealerId, DateTime date);
        //check overlapping time slot
        Task<bool>? IsTimeSlotOverLappingAsync(Guid dealerId, DateTime startTime, DateTime endTime);
        Task<int> CountOverLappingAsync(Guid dealerId, DateTime startTime, DateTime endTime);
    }
}
