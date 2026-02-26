using Microsoft.AspNetCore.Identity;
using Voltix.Domain.Constants;

namespace Voltix.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = {
                StaticUserRole.Admin,
                StaticUserRole.EVMStaff,
                StaticUserRole.DealerStaff,
                StaticUserRole.DealerManager
            };

            foreach (var role in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}

