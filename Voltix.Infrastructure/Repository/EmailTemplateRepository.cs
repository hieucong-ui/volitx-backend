using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class EmailTemplateRepository : Repository<EmailTemplate>, IEmailTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        public EmailTemplateRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EmailTemplate?> GetByNameAsync(string name)
        {
            return await _context.EmailTemplates.FirstOrDefaultAsync(e => e.TemplateName == name);
        }
    }
}
