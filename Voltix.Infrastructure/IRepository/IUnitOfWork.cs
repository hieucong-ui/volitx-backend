using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.IRepository
{
    public interface IUnitOfWork
    {
        public IUserManagerRepository UserManagerRepository { get; }
        public IEmailTemplateRepository EmailTemplateRepository { get; }
        public ICustomerOrderRepository CustomerOrderRepository { get; }
        public IDealerRepository DealerRepository { get; }
        public IElectricVehicleColorRepository ElectricVehicleColorRepository { get; }
        public IElectricVehicleModelRepository ElectricVehicleModelRepository { get; }
        public IElectricVehicleVersionRepository ElectricVehicleVersionRepository { get; }
        public IElectricVehicleRepository ElectricVehicleRepository { get; }
        public IEContractTemplateRepository EContractTemplateRepository { get; }
        public IEContractRepository EContractRepository { get; }
        public IBookingEVRepository BookingEVRepository { get; }
        public IEVCInventoryRepository EVCInventoryRepository { get; }
        public IWarehouseRepository WarehouseRepository { get; }
        public IQuoteRepository QuoteRepository { get; }
        public IPromotionRepository PromotionRepository { get; }
        public IEVAttachmentRepository EVAttachmentRepository { get; }
        public IDealerMemberRepository DealerMemberRepository { get; }
        public IEVTemplateRepository EVTemplateRepository { get; }
        public IBookingDetailRepository BookingDetailRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IAppointmentRepository AppointmentRepository { get; }
        public IDealerFeedbackRepository DealerFeedbackRepository { get; }
        public ITransactionRepository TransactionRepository { get; }
        public INotificationRepository NotificationRepository { get; }
        public IOrderDetailRepository OrderDetailRepository { get; }
        public IDealerFBAttachmentRepository DealerFBAttachmentRepository { get; }
        public ICustomerFeedbackRepository CustomerFeedbackRepository { get; }
        public ICustomerFBAttachRepository CustomerFBAttachRepository { get; }
        public IDealerTierRepository DealerTierRepository { get; }
        public IDealerPolicyOverrideRepository DealerPolicyOverrideRepository { get; }
        public IDealerDebtRepository DealerDebtRepository { get; }
        public IVehicleDeliveryRepository VehicleDeliveryRepository { get; }
        public IDealerDebtTransactionRepository DealerDebtTransactionRepository { get; }
        public IVehicleDeliveryDetailRepository VehicleDeliveryDetailRepository { get; }
        public IDealerDailyInventoryRepository DealerDailyInventoryRepository { get; }
        public IDealerInventoryForecastRepository DealerInventoryForecastRepository { get; }
        public IDealerInventoryRiskRepository DealerInventoryRiskRepository { get; }
        public ILogRepository LogRepository { get; }
        public IDealerConfigurationRepository DealerConfigurationRepository { get; }

        Task<int> SaveAsync(CancellationToken ct = default);
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default);
    }
}
