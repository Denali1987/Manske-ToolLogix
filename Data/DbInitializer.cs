using Microsoft.EntityFrameworkCore;
using ToolLogix.Models;

namespace ToolLogix.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAssetsAsync(
            ApplicationDbContext context)
        {
            if (await context.Assets.AnyAsync())
            {
                return;
            }

            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetNumber = "D214042A",
                    Description = "High seal",
                    PartNumber = "214042",
                    PressNumber = "SY221",
                    Location = "ESKAY",
                    Status = "Active"
                },
                new Asset
                {
                    AssetNumber = "D347111A",
                    Description = "RCM",
                    PartNumber = "347111",
                    PressNumber = "SB103",
                    Location = "Toolroom",
                    Status = "Active"
                },
                new Asset
                {
                    AssetNumber = "D348452B",
                    Description = "Large Header",
                    PartNumber = "348452",
                    PressNumber = "SB218",
                    Location = "ToolRoom",
                    Status = "Active"
                },
                new Asset
                {
                    AssetNumber = "D202333X",
                    Description = "Sleeve stamping die",
                    PartNumber = "2023339035",
                    PressNumber = "SI306",
                    Location = "Production",
                    Status = "Active"
                }
            };

            context.Assets.AddRange(assets);
            await context.SaveChangesAsync();
        }
    }
}