using Microsoft.EntityFrameworkCore;
using ToolLogix.Models;

namespace ToolLogix.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedSpareInventoryAsync(
            IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // Do not add the samples more than once.
            if (await context.SpareItems.AnyAsync())
            {
                return;
            }

            var primaryAsset = await context.Assets
                .FirstOrDefaultAsync(asset =>
                    asset.AssetNumber == "D202333X");

            primaryAsset ??= await context.Assets
                .OrderBy(asset => asset.Id)
                .FirstOrDefaultAsync();

            int? primaryAssetId = primaryAsset?.Id;

            var spareItems = new List<SpareItem>
            {
                new()
                {
                    DetailNumber = "318",
                    Description = "Cutoff punch insert",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet A - Drawer 1",
                    QuantityOnHand = 12,
                    MinimumQuantity = 4,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "298",
                    Description = "Form insert",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet A - Drawer 2",
                    QuantityOnHand = 3,
                    MinimumQuantity = 3,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "P-1042",
                    Description = "Pierce punch",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet B - Drawer 4",
                    QuantityOnHand = 0,
                    MinimumQuantity = 2,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "DB-441",
                    Description = "Die button",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet B - Drawer 5",
                    QuantityOnHand = 8,
                    MinimumQuantity = 4,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "SPR-075",
                    Description = "Heavy-duty die spring",
                    AssetId = null,
                    StorageLocation = "Spring Cabinet - Shelf 2",
                    QuantityOnHand = 24,
                    MinimumQuantity = 10,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "PLT-220",
                    Description = "Round pilot",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet C - Drawer 3",
                    QuantityOnHand = 2,
                    MinimumQuantity = 2,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "GUIDE-114",
                    Description = "Stock guide",
                    AssetId = null,
                    StorageLocation = "Rack 3 - Bin 8",
                    QuantityOnHand = 6,
                    MinimumQuantity = 2,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "SEN-M8",
                    Description = "M8 proximity sensor",
                    AssetId = null,
                    StorageLocation = "Electrical Cabinet - Bin 6",
                    QuantityOnHand = 1,
                    MinimumQuantity = 1,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "SHIM-005",
                    Description = "0.005-inch hardened shim",
                    AssetId = null,
                    StorageLocation = "Shim Cabinet - Drawer 1",
                    QuantityOnHand = 40,
                    MinimumQuantity = 15,
                    LastUpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    DetailNumber = "STRIP-062",
                    Description = "Stripper insert",
                    AssetId = primaryAssetId,
                    StorageLocation = "Cabinet D - Drawer 7",
                    QuantityOnHand = 0,
                    MinimumQuantity = 2,
                    LastUpdatedAt = DateTime.UtcNow
                }
            };

            context.SpareItems.AddRange(spareItems);
            await context.SaveChangesAsync();
        }
    }
}