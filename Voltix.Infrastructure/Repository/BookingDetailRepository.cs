using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class BookingDetailRepository : Repository<BookingEVDetail>, IBookingDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookingEVDetail>> GetBookingDetailsByBookingIdAsync(Guid bookingId, CancellationToken ct)
        {
            return await _context.BookingEVDetails
                .Include(bd => bd.Version)
                    .ThenInclude(v => v.Model)
                .Include(bd => bd.Color)
                .Where(bd => bd.BookingId == bookingId)
                .ToListAsync(ct);
        }
    }
}
