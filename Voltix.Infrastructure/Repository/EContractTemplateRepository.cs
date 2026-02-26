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
    public class EContractTemplateRepository : Repository<EContractTemplate>, IEContractTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EContractTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EContractTemplate?> GetbyCodeAsync(string code, CancellationToken token)
            => await _context.EContractTemplates.FirstOrDefaultAsync(v => v.Code == code, token);

        public async Task<EContractTemplate?> GetbyIdAsync(Guid id, CancellationToken token)
            => await _context.EContractTemplates.FirstOrDefaultAsync(v => v.Id == id, token);
    }
}
