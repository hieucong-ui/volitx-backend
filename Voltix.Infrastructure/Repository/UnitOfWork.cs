using Microsoft.AspNetCore.Identity;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserManagerRepository UserManagerRepository { get; private set; }
        public IEmailTemplateRepository EmailTemplateRepository { get; private set; }
        public ICustomerOrderRepository CustomerOrderRepository { get; private set; }
        public IDealerRepository DealerRepository { get; private set; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; private set; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; private set; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; private set; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; private set; }
        public IEContractTemplateRepository EContractTemplateRepository { get; private set; }
        public IEContractRepository EContractRepository { get; private set; }
        public IBookingEVRepository BookingEVRepository { get; private set; }
        public IEVCInventoryRepository EVCInventoryRepository { get; private set; }
        public IWarehouseRepository WarehouseRepository { get; private set; }
        public IQuoteRepository QuoteRepository { get; private set; }
        public IPromotionRepository PromotionRepository { get; private set; }
        public IEVAttachmentRepository EVAttachmentRepository { get; private set; }
        public IDealerMemberRepository DealerMemberRepository { get; private set; }
        public IEVTemplateRepository EVTemplateRepository { get; private set; }
        public IBookingDetailRepository BookingDetailRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public IAppointmentRepository AppointmentRepository { get; private set; }
        public ITransactionRepository TransactionRepository { get; private set; }
        public INotificationRepository NotificationRepository { get; private set; }
        public IOrderDetailRepository OrderDetailRepository { get; private set; }
        public IDealerFeedbackRepository DealerFeedbackRepository { get; private set; }
        public IDealerFBAttachmentRepository DealerFBAttachmentRepository { get; private set; }
        public ICustomerFeedbackRepository CustomerFeedbackRepository { get; private set; }
        public ICustomerFBAttachRepository CustomerFBAttachRepository { get; private set; }
        public IDealerTierRepository DealerTierRepository { get; private set; }
        public IDealerPolicyOverrideRepository DealerPolicyOverrideRepository { get; private set; }
        public IDealerDebtRepository DealerDebtRepository { get; private set; }
        public IVehicleDeliveryRepository VehicleDeliveryRepository { get; private set; }
        public IDealerDebtTransactionRepository DealerDebtTransactionRepository { get; private set; }
        public IVehicleDeliveryDetailRepository VehicleDeliveryDetailRepository { get; private set; }
        public IDealerDailyInventoryRepository DealerDailyInventoryRepository { get; private set; }
        public IDealerInventoryForecastRepository DealerInventoryForecastRepository { get; private set; }
        public IDealerInventoryRiskRepository DealerInventoryRiskRepository { get; private set; }
        public ILogRepository LogRepository { get; private set; }
        public IDealerConfigurationRepository DealerConfigurationRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManagerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            UserManagerRepository = new UserManagerRepository(userManagerRepository, _context);
            EmailTemplateRepository = new EmailTemplateRepository(_context);
            CustomerOrderRepository = new CustomerOrderRepository(_context);
            DealerRepository = new DealerRepository(_context);
            ElectricVehicleColorRepository = new ElectricVehicleColorRepository(_context);
            ElectricVehicleModelRepository = new ElectricVehicleModelRepository(_context);
            ElectricVehicleVersionRepository = new ElectricVehicleVersionRepository(_context);
            ElectricVehicleRepository = new ElectricVehicleRepository(_context);
            EContractTemplateRepository = new EContractTemplateRepository(_context);
            EContractRepository = new EContractRepository(_context);
            BookingEVRepository = new BookingEVRepository(_context);
            EVCInventoryRepository = new EVCInventoryRepository(_context);
            WarehouseRepository = new WarehouseRepository(_context);
            QuoteRepository = new QuoteRepository(_context);
            PromotionRepository = new PromotionRepository(_context);
            EVAttachmentRepository = new EVAttachmentRepository(_context);
            DealerMemberRepository = new DealerMemberRepository(_context);
            EVTemplateRepository = new EVTemplateRepository(_context);
            BookingDetailRepository = new BookingDetailRepository(_context);
            CustomerRepository = new CustomerRepository(_context);
            AppointmentRepository = new AppointmentRepository(_context);
            TransactionRepository = new TransactionRepository(_context);
            NotificationRepository = new NotificationRepository(_context);
            OrderDetailRepository = new OrderDetailRepository(_context);
            DealerFeedbackRepository = new DealerFeedbackRepository(_context);
            DealerFBAttachmentRepository = new DealerFBAttachmentRepository(_context);
            CustomerFeedbackRepository = new CustomerFeedbackRepository(_context);
            CustomerFBAttachRepository = new CustomerFBAttachRepository(_context);
            DealerTierRepository = new DealerTierRepository(_context);
            DealerPolicyOverrideRepository = new DealerPolicyOverrideRepository(_context);
            DealerDebtRepository = new DealerDebtRepository(_context);
            VehicleDeliveryRepository = new VehicleDeliveryRepository(_context);
            DealerDebtTransactionRepository = new DealerDebtTransactionRepository(_context);
            VehicleDeliveryDetailRepository = new VehicleDeliveryDetailRepository(_context);
            DealerDailyInventoryRepository = new DealerDailyInventoryRepository(_context);
            DealerInventoryForecastRepository = new DealerInventoryForecastRepository(_context);
            DealerInventoryRiskRepository = new DealerInventoryRiskRepository(_context);
            LogRepository = new LogRepository(_context);
            DealerConfigurationRepository = new DealerConfigurationRepository(_context);
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default)
        {
           await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await action();
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
