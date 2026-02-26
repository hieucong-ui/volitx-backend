using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Seeders;

namespace Voltix.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<ElectricVehicleColor> ElectricVehicleColors { get; set; }
        public DbSet<ElectricVehicleModel> ElectricVehicleModels { get; set; }
        public DbSet<ElectricVehicleVersion> ElectricVehicleVersions { get; set; }
        public DbSet<ElectricVehicle> ElectricVehicles { get; set; }
        public DbSet<EContract> EContracts { get; set; }
        public DbSet<EContractTemplate> EContractTemplates { get; set; }
        public DbSet<BookingEV> BookingEVs { get; set; }
        public DbSet<BookingEVDetail> BookingEVDetails { get; set; }
        public DbSet<EVCInventory> EVCInventories { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<EVAttachment> EVAttachments { get; set; }
        public DbSet<QuoteDetail> QuoteDetails { get; set; }
        public DbSet<ElectricVehicleTemplate> ElectricVehicleTemplates { get; set; }
        public DbSet<DealerMember> DealerMembers { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<DealerFeedback> DealerFeedbacks { get; set; }
        public DbSet<DealerFBAttachment> DealerFBAttachments { get; set; }
        public DbSet<CustomerFBAttachment> CustomerFBAttachments { get; set; }
        public DbSet<DealerPolicyOverride> DealerPolicyOverrides { get; set; }
        public DbSet<DealerDebt> DealerDebts { get; set; }
        public DbSet<DealerTier> DealerTiers { get; set; }
        public DbSet<VehicleDelivery> VehicleDeliveries { get; set; }
        public DbSet<VehicleDeliveryDetail> VehicleDeliveryDetails { get; set; }
        public DbSet<DealerDebtTransaction> DealerDebtTransactions { get; set; }
        public DbSet<DealerDailyInventory> DealerDailyInventories { get; set; }
        public DbSet<DealerInventoryForecast> DealerInventoryForecasts { get; set; }
        public DbSet<DealerInventoryRisk> DealerInventoryRisks { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<DealerConfiguration> DealerConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Seed initial data
            DealerTierSeeder.DealerTierConfigure(modelBuilder);
            EmailSeeder.SeedEmailTemplate(modelBuilder);
            EContractSeeder.EContractTemplateSeeder.SeedDealerEContract(modelBuilder);
            AdminSeeder.AdminConfigure(modelBuilder);

            // Customize ASP.NET Identity table names
            modelBuilder.Entity<ApplicationUser>(b =>
                b.ToTable("ApplicationUsers"));

            modelBuilder.Entity<IdentityRole>(b =>
                b.ToTable("Roles"));

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
                b.ToTable("UserRoles"));

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
                b.ToTable("RoleClaims"));

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
                b.ToTable("UserClaims"));

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
                b.ToTable("UserLogins"));

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
                b.ToTable("UserTokens"));

            /******************************************************************************/
            // Configure Dealer entity

            modelBuilder.Entity<Dealer>()
                .HasOne(d => d.DealerTier)
                .WithMany(dd => dd.Dealers)
                .HasForeignKey(dd => dd.DealerTierId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure DealerDebt entity

            modelBuilder.Entity<DealerDebt>()
                .HasOne(dd => dd.Dealer)
                .WithMany(d => d.DealerDebts)
                .HasForeignKey(dd => dd.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure DealerPolicyOverride entity

            modelBuilder.Entity<DealerPolicyOverride>()
                .HasOne(dpo => dpo.Dealer)
                .WithMany(d => d.PolicyOverrides)
                .HasForeignKey(dpo => dpo.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Customer entity


            modelBuilder.Entity<Customer>()
                .HasMany(d => d.Dealers)
                .WithMany(u => u.Customers)
                .UsingEntity<Dictionary<string, object>>(
                    "DealerCustomers",
                    j => j
                        .HasOne<Dealer>()
                        .WithMany()
                        .HasForeignKey("DealerId")
                        .HasConstraintName("FK_DealerCustomers_Dealers_DealerId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<Customer>()
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .HasConstraintName("FK_DealerCustomers_Customers_CustomerId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j =>
                    {
                        j.HasKey("DealerId", "CustomerId");
                        j.Property<Guid>("DealerId");
                        j.Property<Guid>("CustomerId");
                        j.ToTable("DealerCustomers");
                        j.HasIndex("DealerId");
                        j.HasIndex("CustomerId");
                    });


            // Dealer - Manager (ApplicationUser) one-to-many relationship
            modelBuilder.Entity<Dealer>()
                .HasOne(d => d.Manager)
                .WithMany(m => m.ManagingDealers)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on ManagerId in Dealer for performance
            modelBuilder.Entity<Dealer>()
                .HasIndex(d => d.ManagerId);

            /******************************************************************************/
            // Configure ElectricVehicle entity

            modelBuilder.Entity<ElectricVehicle>()
                .HasOne(ev => ev.ElectricVehicleTemplate)
                .WithMany(vs => vs.ElectricVehicles)
                .HasForeignKey(ev => ev.ElectricVehicleTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicle>()
                .HasOne(ev => ev.Warehouse)
                .WithMany(d => d.ElectricVehicles)
                .HasForeignKey(ev => ev.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicle>()
                .HasIndex(ev => ev.VIN)
                .IsUnique();

            /******************************************************************************/
            // Configure ElectricVehicleVersion entity

            modelBuilder.Entity<ElectricVehicleVersion>()
                .HasOne(vs => vs.Model)
                .WithMany(ev => ev.Versions)
                .HasForeignKey(ev => ev.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure BookingEVDetail entity

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.BookingEV)
                .WithMany(b => b.BookingEVDetails)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.Version)
                .WithMany(v => v.BookingEVDetails)
                .HasForeignKey(bd => bd.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.Color)
                .WithMany(c => c.BookingEVDetails)
                .HasForeignKey(bd => bd.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure BookingEV entity

            modelBuilder.Entity<BookingEV>()
                .HasOne(b => b.Dealer)
                .WithMany(d => d.BookingEVs)
                .HasForeignKey(b => b.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingEV>()
                .HasOne(b => b.EContract)
                .WithOne(e => e.BookingEV)
                .HasForeignKey<BookingEV>(b => b.EContractId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure EContract entity

            modelBuilder.Entity<EContract>()
                .HasOne(e => e.Owner)
                .WithMany(o => o.EContracts)
                .HasForeignKey(e => e.OwnerBy)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Warehouse entity

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Dealer)
                .WithOne(d => d.Warehouse)
                .HasForeignKey<Warehouse>(w => w.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.EVCInventory)
                .WithOne(d => d.Warehouse)
                .HasForeignKey<Warehouse>(w => w.EVCInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure EVAttachment entity

            modelBuilder.Entity<EVAttachment>()
                .HasOne(eva => eva.ElectricVehicleTemplate)
                .WithMany(ev => ev.EVAttachments)
                .HasForeignKey(eva => eva.ElectricVehicleTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Quote entity

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Dealer)
                .WithMany(d => d.Quotes)
                .HasForeignKey(q => q.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.CreatedByUser)
                .WithMany(u => u.Quotes)
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure QuoteDetail entity

            modelBuilder.Entity<QuoteDetail>()
                .HasOne(qd => qd.Quote)
                .WithMany(q => q.QuoteDetails)
                .HasForeignKey(qd => qd.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuoteDetail>()
                .HasOne(qd => qd.ElectricVehicleVersion)
                .WithMany(v => v.QuoteDetails)
                .HasForeignKey(qd => qd.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuoteDetail>()
                .HasOne(qd => qd.ElectricVehicleColor)
                .WithMany(c => c.QuoteDetails)
                .HasForeignKey(qd => qd.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuoteDetail>()
                .HasOne(qd => qd.Promotion)
                .WithMany(p => p.QuoteDetails)
                .HasForeignKey(qd => qd.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);


            /******************************************************************************/
            // Configure Promotion entity

            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Model)
                .WithOne(m => m.Promotion)
                .HasForeignKey<Promotion>(p => p.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Promotion>()
                .HasOne(p => p.Version)
                .WithOne(v => v.Promotion)
                .HasForeignKey<Promotion>(p => p.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure DealerMember entity

            modelBuilder.Entity<DealerMember>()
                .HasKey(dm => new { dm.DealerId, dm.ApplicationUserId });

            modelBuilder.Entity<DealerMember>()
                .HasOne(dm => dm.Dealer)
                .WithMany(d => d.DealerMembers)
                .HasForeignKey(dm => dm.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerMember>()
                .HasOne(dm => dm.ApplicationUser)
                .WithMany(au => au.DealerMembers)
                .HasForeignKey(dm => dm.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure ElectricVehicleTemplate entity

            modelBuilder.Entity<ElectricVehicleTemplate>()
                .HasOne(evt => evt.Version)
                .WithMany(vs => vs.ElectricVehicleTemplates)
                .HasForeignKey(evt => evt.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicleTemplate>()
                .HasOne(evt => evt.Color)
                .WithMany(c => c.ElectricVehicleTemplates)
                .HasForeignKey(evt => evt.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure CustomerOrder entity

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.Quote)
                .WithMany(q => q.CustomerOrders)
                .HasForeignKey(co => co.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.Customer)
                .WithMany(c => c.CustomerOrders)
                .HasForeignKey(co => co.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(co => co.CreatedByUser)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(co => co.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure OrderDetail entity

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.CustomerOrder)
                .WithMany(co => co.OrderDetails)
                .HasForeignKey(od => od.CustomerOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ElectricVehicle)
                .WithMany(v => v.OrderDetails)
                .HasForeignKey(od => od.ElectricVehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Transaction entity

            modelBuilder.Entity<Transaction>()
                .HasOne(tr => tr.CustomerOrder)
                .WithMany(co => co.Transactions)
                .HasForeignKey(tr => tr.CustomerOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Appointment entity

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Dealer)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.EVTemplate)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.EVTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure Notification entity

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Dealer)
                .WithMany(d => d.Notifications)
                .HasForeignKey(n => n.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure CustomerFeedback entity

            modelBuilder.Entity<CustomerFeedback>()
                .HasOne(cf => cf.Customer)
                .WithMany(c => c.CustomerFeedbacks)
                .HasForeignKey(cf => cf.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerFeedback>()
                .HasOne(cf => cf.Dealer)
                .WithMany(d => d.CustomerFeedbacks)
                .HasForeignKey(cf => cf.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure DealerFeedback entity

            modelBuilder.Entity<DealerFeedback>()
                .HasOne(df => df.Dealer)
                .WithMany(d => d.DealerFeedbacks)
                .HasForeignKey(df => df.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure DealerFBAttachment entity

            modelBuilder.Entity<DealerFBAttachment>()
                .HasOne(dfba => dfba.DealerFeedback)
                .WithMany(df => df.DealerFBAttachments)
                .HasForeignKey(dfba => dfba.DealerFeedBackId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure CustomerFBAttachment entity

            modelBuilder.Entity<CustomerFBAttachment>()
                .HasOne(cfba => cfba.CustomerFeedback)
                .WithMany(cf => cf.CustomerFBAttachments)
                .HasForeignKey(cfba => cfba.CustomerFeedBackId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure VehicleDeliveryDetail entity

            modelBuilder.Entity<VehicleDeliveryDetail>()
                .HasOne(vdd => vdd.VehicleDelivery)
                .WithMany(vd => vd.VehicleDeliveryDetails)
                .HasForeignKey(vdd => vdd.VehicleDeliveryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleDeliveryDetail>()
                .HasOne(vdd => vdd.ElectricVehicle)
                .WithMany(ev => ev.VehicleDeliveryDetails)
                .HasForeignKey(vdd => vdd.ElectricVehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            /*****************************************************************************/
            // Configure DealerDebtTransaction entity

            modelBuilder.Entity<DealerDebtTransaction>()
                .HasOne(ddt => ddt.Dealer)
                .WithMany(d => d.DealerDebtTransactions)
                .HasForeignKey(ddt => ddt.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerDebt>(e =>
            {
                e.HasIndex(x => new { x.DealerId, x.PeriodFrom, x.PeriodTo }).IsUnique();
            });

            /******************************************************************************/
            // Configure DealerDailyInventory entity

            modelBuilder.Entity<DealerDailyInventory>(e =>
            {
                e.HasIndex(x => new { x.DealerId, x.EVTemplateId, x.SnapshotDate }).IsUnique();

                e.Property(x => x.SnapshotDate).HasColumnType("date");

                e.HasOne(x => x.Dealer)
                 .WithMany(d => d.DealerDailyInventories)
                 .HasForeignKey(x => x.DealerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.EVTemplate)
                 .WithMany(ev => ev.DealerDailyInventories)
                 .HasForeignKey(x => x.EVTemplateId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            /******************************************************************************/
            // Configure DealerInventoryForecast entity

            modelBuilder.Entity<DealerInventoryForecast>(e =>
            {
                e.HasIndex(x => new { x.DealerId, x.EVTemplateId, x.TargetDate });

                e.HasOne(x => x.Dealer)
                 .WithMany(d => d.DealerInventoryForecasts)
                 .HasForeignKey(x => x.DealerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.EVTemplate)
                 .WithMany(ev => ev.DealerInventoryForecasts)
                 .HasForeignKey(x => x.EVTemplateId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            /******************************************************************************/
            // Configure DealerInventoryRisk entity

            modelBuilder.Entity<DealerInventoryRisk>(e =>
            {
                e.HasOne(x => x.Dealer)
                 .WithMany(d => d.DealerInventoryRisks)
                 .HasForeignKey(x => x.DealerId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.EVTemplate)
                 .WithMany(ev => ev.DealerInventoryRisks)
                 .HasForeignKey(x => x.EVTemplateId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            /******************************************************************************/
            // Configure Log entity

            modelBuilder.Entity<Log>(e =>
            {
                e.HasOne(l => l.User)
                 .WithMany(u => u.Logs)
                 .HasForeignKey(l => l.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(l => l.Dealer)
                 .WithMany(d => d.Logs)
                 .HasForeignKey(l => l.DealerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            /******************************************************************************/
            // Configure DealerConfiguration entity

            modelBuilder.Entity<DealerConfiguration>()
                .HasOne(dc => dc.Dealer)
                .WithOne(d => d.DealerConfiguration)
                .HasForeignKey<DealerConfiguration>(dc => dc.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerConfiguration>()
                .HasOne(dc => dc.Manager)
                .WithOne(d => d.DealerConfiguration)
                .HasForeignKey<DealerConfiguration>(dc => dc.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
