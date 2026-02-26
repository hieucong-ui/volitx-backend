using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct);
        Task MarkAllAsReadAsync(Guid dealerId, string targetRole, CancellationToken ct);
    }
}
