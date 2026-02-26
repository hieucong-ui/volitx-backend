using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Seeders
{
    public class AdminSeeder
    {
        public static void AdminConfigure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DealerConfiguration>().HasData(
                new
                {
                    Id = Guid.Parse("7b685c2e-3f4e-4d2a-9f4b-2c3d5e6f7a8b"),
                    ManagerId = "11582b41-2fde-4c54-a978-c181fd71bd6c",
                    DealerId = (Guid?)null,
                    AllowOverlappingAppointments = false,
                    MaxConcurrentAppointments = 1,
                    OpenTime = new TimeSpan(9, 0, 0),
                    CloseTime = new TimeSpan(17, 0, 0),
                    MinIntervalBetweenAppointments = 60,
                    BreakTimeBetweenAppointments = 15,
                    MinDepositPercentage = 5.0m,
                    MaxDepositPercentage = 20.0m,
                    DayCancelDeposit = 7,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
