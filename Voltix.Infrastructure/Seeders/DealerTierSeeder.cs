using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Seeders
{
    public static class DealerTierSeeder
    {
        public static void DealerTierConfigure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DealerTier>().HasData(
                new
                {
                    Id = Guid.Parse("c937f7f4-6d34-417b-8979-273318f2ce0b"),
                    Level = 5,
                    Name = "Tier 5 - Entry Dealer",
                    BaseCommissionPercent = 1.0m,
                    BaseCreditLimit = 100_000_000m,
                    BaseLatePenaltyPercent = 1.5m,
                    BaseDepositPercent = 10.0m,
                    Description = "Đại lý mới / doanh số thấp / cần theo dõi chặt.",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },

                new
                {
                    Id = Guid.Parse("49a2e2a7-4c7a-42fe-9876-d029bbb48758"),
                    Level = 4,
                    Name = "Tier 4 - Standard Dealer",
                    BaseCommissionPercent = 2.0m,
                    BaseCreditLimit = 300_000_000m,
                    BaseLatePenaltyPercent = 1.2m,
                    BaseDepositPercent = 8.0m,
                    Description = "Đại lý hoạt động ổn định, đã có doanh số đều.",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },

                new
                {
                    Id = Guid.Parse("e1fd021c-5db8-4701-847d-18dd34f32a1f"),
                    Level = 3,
                    Name = "Tier 3 - Silver Dealer",
                    BaseCommissionPercent = 3.0m,
                    BaseCreditLimit = 600_000_000m,
                    BaseLatePenaltyPercent = 1.0m,
                    BaseDepositPercent = 6.0m,
                    Description = "Đại lý có doanh số khá, có lịch sử thanh toán tốt.",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },

                new
                {
                    Id = Guid.Parse("43f94aed-a0d8-4d07-8f87-ece8a1104015"),
                    Level = 2,
                    Name = "Tier 2 - Gold Dealer",
                    BaseCommissionPercent = 4.0m,
                    BaseCreditLimit = 1_000_000_000m,
                    BaseLatePenaltyPercent = 0.8m,
                    BaseDepositPercent = 4.0m,
                    Description = "Đại lý lớn, thanh toán đúng hạn, được ưu đãi tốt hơn.",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },

                new
                {
                    Id = Guid.Parse("4adccdc3-6796-4deb-953c-c317b548146d"),
                    Level = 1,
                    Name = "Tier 1 - Key / Strategic Dealer",
                    BaseCommissionPercent = 5.0m,
                    BaseCreditLimit = 2_000_000_000m,
                    BaseLatePenaltyPercent = 0.5m,
                    BaseDepositPercent = 2.0m,
                    Description = "Đại lý chiến lược / doanh số rất cao / cần ưu tiên.",
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                }
            );
        }
    }
}
