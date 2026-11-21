using BiristaCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        public DbSet<CompanyMaster> CompanyMasters { get; set; }

        public DbSet<UserMaster> UserMasters { get; set; }

        public DbSet<ProductMaster> ProductMasters { get; set; }

        public DbSet<ClientMaster> ClientMasters { get; set; }

        public DbSet<VisitMaster> VisitMasters { get; set; }

        public DbSet<OrderMaster> OrderMasters { get; set; }
    }
}
