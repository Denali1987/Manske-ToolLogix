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

        public async Task<IActionResult> Details(
            int id,
            int laborDays = 1)
        {
            laborDays = Math.Clamp(laborDays, 1, 365);

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
                .CountAsync(workLog =>
                    workLog.AssetId == id);

            /*
             * Load the ten most recent work logs for the
             * full-width work-log card.
             */
            var workLogs = await _context.WorkLogs
                .AsNoTracking()
                .Where(workLog =>
                    workLog.AssetId == id)
                .Include(workLog =>
                    workLog.LaborEntries)
                .Include(workLog =>
                    workLog.DetailsUsed)
                .AsSplitQuery()
                .OrderByDescending(workLog =>
                    workLog.WorkDate)
                .ThenByDescending(workLog =>
                    workLog.CreatedAt)
                .Take(recentWorkLogLimit)
                .ToListAsync();

            /*
             * Load labor from the selected rolling period.
             * One day means the previous 24 hours.
             */
            var laborStartUtc = DateTime.UtcNow
                .AddDays(-laborDays);

            var laborCharges = await _context.LaborEntries
                .AsNoTracking()
                .Where(entry =>
                    entry.WorkLog.AssetId == id &&
                    entry.WorkLog.CreatedAt >= laborStartUtc)
                .Include(entry =>
                    entry.WorkLog)
                .OrderByDescending(entry =>
                    entry.WorkLog.CreatedAt)
                .ThenByDescending(entry =>
                    entry.Id)
                .ToListAsync();

            /*
             * Load the ten most recently used details.
             */
            var recentDetailUsages =
                await _context.DetailUsages
                    .AsNoTracking()
                    .Where(detail =>
                        detail.WorkLog.AssetId == id)
                    .Include(detail =>
                        detail.WorkLog)
                    .OrderByDescending(detail =>
                        detail.WorkLog.WorkDate)
                    .ThenByDescending(detail =>
                        detail.Id)
                    .Take(10)
                    .ToListAsync();

            /*
             * Collect every user ID needed by all three
             * asset-dashboard sections.
             */
            var relatedUserIds = workLogs
                .SelectMany(workLog =>
                    workLog.LaborEntries)
                .Select(entry =>
                    entry.UserId)
                .Concat(
                    laborCharges.Select(entry =>
                        entry.UserId))
                .Concat(
                    recentDetailUsages.Select(detail =>
                        detail.UsedByUserId ??
                        detail.WorkLog.CreatedByUserId))
                .Where(userId =>
                    !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .AsNoTracking()
                .Where(user =>
                    relatedUserIds.Contains(user.Id))
                .ToDictionaryAsync(
                    user => user.Id,
                    user =>
                        user.UserName ??
                        user.Email ??
                        "Unknown user");

            var viewModel = new AssetDetailsViewModel
            {
                Asset = asset,

                TotalWorkLogCount = totalWorkLogCount,

                LaborDays = laborDays,

                LaborTotalHours = laborCharges
                    .Sum(entry => entry.Hours),

                LaborCharges = laborCharges
                    .Select(entry =>
                        new AssetLaborChargeViewModel
                        {
                            WorkDate =
                                entry.WorkLog.WorkDate,

                            EmployeeName =
                                userNames.TryGetValue(
                                    entry.UserId,
                                    out var employeeName)
                                        ? employeeName
                                        : "Unknown user",

                            WorkOrderNumber =
                                entry.WorkLog.WorkOrderNumber,

                            Hours = entry.Hours
                        })
                    .ToList(),

                RecentDetailUsages = recentDetailUsages
                    .Select(detail =>
                    {
                        var usedByUserId =
                            detail.UsedByUserId ??
                            detail.WorkLog.CreatedByUserId;

                        return new AssetDetailUsageViewModel
                        {
                            WorkDate =
                                detail.WorkLog.WorkDate,

                            DetailNumber =
                                detail.DetailNumber,

                            Description =
                                detail.Description,

                            Quantity =
                                detail.Quantity,

                            UsedByName =
                                userNames.TryGetValue(
                                    usedByUserId,
                                    out var usedByName)
                                        ? usedByName
                                        : "Unknown user"
                        };
                    })
                    .ToList(),

                RecentWorkLogs = workLogs
                    .Select(workLog =>
                        new WorkLogRowViewModel
                        {
                            Id = workLog.Id,

                            WorkDate =
                                workLog.WorkDate,

                            AssetNumber =
                                asset.AssetNumber,

                            ProductNumber =
                                asset.PartNumber,

                            WorkOrderNumber =
                                workLog.WorkOrderNumber,

                            Issue =
                                workLog.Issue,

                            WorkDone =
                                workLog.WorkDone,

                            Notes =
                                workLog.Notes,

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
                                workLog.DetailsUsed
                                    .Select(detail =>
                                        $"{detail.DetailNumber} × " +
                                        $"{detail.Quantity}")),

                            TotalHours =
                                workLog.LaborEntries
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