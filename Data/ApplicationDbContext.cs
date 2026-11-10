using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToolLogix.Models;

namespace ToolLogix.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShopAnnouncement> ShopAnnouncements
            => Set<ShopAnnouncement>();

        public DbSet<Asset> Assets
            => Set<Asset>();

        public DbSet<WorkLog> WorkLogs
            => Set<WorkLog>();

        public DbSet<LaborEntry> LaborEntries
            => Set<LaborEntry>();

        public DbSet<DetailUsage> DetailUsages
            => Set<DetailUsage>();

        public DbSet<SpareItem> SpareItems
            => Set<SpareItem>();

        public DbSet<InventoryTransaction> InventoryTransactions
            => Set<InventoryTransaction>();
    }
}

