using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Models;
using BiristaCRM.Services;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();

            const string superAdminEmail = "superadmin@biristacrm.com";

            var exists = await db.UserMasters
                .AnyAsync(u => u.Email == superAdminEmail && !u.IsDeleted);

            if (exists)
            {
                return;
            }

            var superAdmin = new UserMaster
            {
                Id = Guid.NewGuid(),
                CompanyId = Guid.Empty,
                FullName = "Super Admin",
                Email = superAdminEmail,
                Password = PasswordHasher.Hash("SuperAdmin@123"),
                Mobile = "9999999999",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.UserMasters.Add(superAdmin);
            await db.SaveChangesAsync();
        }
    }
}
