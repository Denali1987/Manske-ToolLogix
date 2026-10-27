using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToolLogix.Data;
using ToolLogix.Models;

namespace ToolLogix.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .FirstOrDefaultAsync(asset => asset.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            const int recentWorkLogLimit = 10;

            var totalWorkLogCount = await _context.WorkLogs
                .AsNoTracking()
                .CountAsync(workLog => workLog.AssetId == id);

            var workLogs = await _context.WorkLogs
                .AsNoTracking()
                .Where(workLog => workLog.AssetId == id)
                .Include(workLog => workLog.LaborEntries)
                .Include(workLog => workLog.DetailsUsed)
                .AsSplitQuery()
                .OrderByDescending(workLog => workLog.WorkDate)
                .ThenByDescending(workLog => workLog.CreatedAt)
                .Take(recentWorkLogLimit)
                .ToListAsync();

            var userIds = workLogs
                .SelectMany(workLog => workLog.LaborEntries)
                .Select(entry => entry.UserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.UserName ?? user.Email ?? "Unknown user");

            var viewModel = new AssetDetailsViewModel
            {
                Asset = asset,

                TotalWorkLogCount = totalWorkLogCount,

                RecentWorkLogs = workLogs.Select(workLog =>
                    new WorkLogRowViewModel
                    {
                        Id = workLog.Id,
                        WorkDate = workLog.WorkDate,
                        AssetNumber = asset.AssetNumber,
                        ProductNumber = asset.PartNumber,
                        WorkOrderNumber = workLog.WorkOrderNumber,
                        Issue = workLog.Issue,
                        WorkDone = workLog.WorkDone,

                        Employees = string.Join(
                            ", ",
                            workLog.LaborEntries
                                .Select(entry =>
                                    userNames.TryGetValue(
                                        entry.UserId,
                                        out var userName)
                                            ? userName
                                            : "Unknown user")
                                .Distinct()),

                        DetailsUsed = string.Join(
                            ", ",
                            workLog.DetailsUsed.Select(detail =>
                                $"{detail.DetailNumber} × {detail.Quantity}")),

                        TotalHours = workLog.LaborEntries
                            .Sum(entry => entry.Hours)
                    })
                    .ToList(),

                // Temporary test print.
                PrintUrl =
                    "https://www.diedesigner.com/uploads/5/0/8/1/50813613/formdie.pdf"
            };

            return View(viewModel);
        }
    }
}