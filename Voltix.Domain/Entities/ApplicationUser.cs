using Microsoft.AspNetCore.Identity;

namespace Voltix.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Dealer> ManagingDealers { get; set; } = new List<Dealer>();

        public ICollection<EContract> EContracts { get; set; } = new List<EContract>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<DealerMember> DealerMembers { get; set; } = new List<DealerMember>();
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        public ICollection<Log>? Logs { get; set; }
        public DealerConfiguration? DealerConfiguration { get; set; }
    }
}
