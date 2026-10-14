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

        public DbSet<ShopAnnouncement> ShopAnnouncements => Set<ShopAnnouncement>();

        public DbSet<Asset> Assets => Set<Asset>();
    }
}

