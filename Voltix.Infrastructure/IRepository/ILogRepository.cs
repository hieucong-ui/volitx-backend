using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface ILogRepository : IRepository<Log>
    {
        Task<List<Log>> GetLogsByCreateAtRange(DateTime startDate , DateTime endDate);
        Task<List<Log>> GetLogsByDealerId(Guid dealerId);
        Task<List<Log>> GetLogsByType(LogType logType);
    }
}
