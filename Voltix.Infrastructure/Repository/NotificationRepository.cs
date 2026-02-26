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
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
        }

        public async Task MarkAllAsReadAsync(Guid dealerId, string targetRole, CancellationToken ct)
        {
            await _context.Notifications
               .Where(n => n.DealerId == dealerId && n.TargetRole == targetRole && !n.IsRead)
               .ForEachAsync(n => n.IsRead = true, ct);
        }
    }
}
