using Voltix.Domain.Entities;

namespace Voltix.Infrastructure.IRepository
{
    public interface IEmailTemplateRepository : IRepository<EmailTemplate>
    {
        Task<EmailTemplate?> GetByNameAsync(string name);
    }
}
