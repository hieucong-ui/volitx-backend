using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class LogRepository : Repository<Log>, ILogRepository
    {
        public readonly ApplicationDbContext _context;
        public LogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Log>> GetLogsByCreateAtRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Logs
                .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Log>> GetLogsByDealerId(Guid dealerId)
        {
            return await _context.Logs
                .Where(x => x.DealerId == dealerId)
                .ToListAsync();
        }

        public async Task<List<Log>> GetLogsByType(LogType logType)
        {
            return await _context.Logs
                .Where(x =>x.LogType == logType)
                .ToListAsync();
        }
    }
}
