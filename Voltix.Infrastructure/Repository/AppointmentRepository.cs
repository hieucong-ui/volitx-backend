using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
       public readonly ApplicationDbContext _context;
        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> CountOverLappingAsync(Guid dealerId, DateTime startTime, DateTime endTime)
        {
            return await _context.Appointments.CountAsync(a =>
                    a.DealerId == dealerId &&
                    a.Status != AppointmentStatus.Canceled && // //Count only active and completed appointments
                    (
                        (startTime >= a.StartTime && startTime < a.EndTime) || // StartTime overlaps
                        (endTime > a.StartTime && endTime <= a.EndTime) ||     // EndTime overlaps
                        (startTime <= a.StartTime && endTime >= a.EndTime)     // Encompasses existing appointment
        )
    );
        }

        public async Task<List<Appointment>> GetAllByDealerIdAsync(Guid dealerId)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.EVTemplate)
                    .ThenInclude(t => t.Version)
                        .ThenInclude(v => v.Model)
                .Include(a => a.EVTemplate.Color)
                .Include(a => a.Dealer)
                .Where(a => a.DealerId == dealerId)
                .OrderBy(a => a.StartTime)
                .ToListAsync();
        }


        public async Task<List<Appointment>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Appointments
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByDealerIdAndDateAsync(Guid dealerId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.Appointments
                .Where(a => a.DealerId == dealerId
                            && a.StartTime >= startOfDay
                            && a.StartTime < endOfDay
                            && a.Status == AppointmentStatus.Active)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetByDealerIdAsync(Guid dealerId)
        {
            return await  _context.Appointments
                .Where(a => a.DealerId == dealerId)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
        //check overlapping time slot
        public async Task<bool>? IsTimeSlotOverLappingAsync(Guid dealerId, DateTime startTime, DateTime endTime)
        {
            return await _context.Appointments
                .AnyAsync(a => a.DealerId == dealerId &&
                               ((startTime >= a.StartTime && startTime < a.EndTime) || //StartTime overlaps
                                (endTime > a.StartTime && endTime <= a.EndTime) || //EndTime overlaps
                                (startTime <= a.StartTime && endTime >= a.EndTime))); //Encompasses existing appointment
        }
    }
}
