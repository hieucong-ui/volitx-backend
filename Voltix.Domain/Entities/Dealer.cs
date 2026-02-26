using Voltix.Domain.Enums;

namespace Voltix.Domain.Entities
{
    public class Dealer
    {
        public Guid Id { get; set; }
        public string? ManagerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string TaxNo { get; set; } = null!;
        public Guid? DealerTierId { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public DealerStatus DealerStatus { get; set; } = DealerStatus.Inactive;

        public DealerTier DealerTier { get; set; } = null!;
        public ApplicationUser? Manager { get; set; }
        public Warehouse Warehouse { get; set; } = null!;
        public ICollection<BookingEV> BookingEVs { get; set; } = new List<BookingEV>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<DealerMember> DealerMembers { get; set; } = new List<DealerMember>();
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<DealerFeedback> DealerFeedbacks { get; set; } = new List<DealerFeedback>();
        public ICollection<CustomerFeedback> CustomerFeedbacks { get; set; } = new List<CustomerFeedback>();
        public ICollection<DealerPolicyOverride> PolicyOverrides { get; set; } = new List<DealerPolicyOverride>();
        public ICollection<DealerDebt> DealerDebts { get; set; } = new List<DealerDebt>();
        public ICollection<DealerDebtTransaction> DealerDebtTransactions { get; set; } = new List<DealerDebtTransaction>();
        public ICollection<DealerDailyInventory> DealerDailyInventories { get; set; } = new List<DealerDailyInventory>();
        public ICollection<DealerInventoryForecast> DealerInventoryForecasts { get; set; } = new List<DealerInventoryForecast>();
        public ICollection<DealerInventoryRisk> DealerInventoryRisks { get; set; } = new List<DealerInventoryRisk>();
        public ICollection<Log>? Logs { get; set; }
        public DealerConfiguration? DealerConfiguration { get; set; }
    }
}
