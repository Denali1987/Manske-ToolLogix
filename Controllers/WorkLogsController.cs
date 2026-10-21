using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToolLogix.Data;
using ToolLogix.Models;
using System.Security.Claims;

namespace ToolLogix.Controllers
{
    [Authorize]
    public class WorkLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? assetId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.WorkLogs
                .AsNoTracking()
                .Include(workLog => workLog.Asset)
                .Include(workLog => workLog.LaborEntries)
                .Include(workLog => workLog.DetailsUsed)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(workLog =>
                    workLog.Asset.AssetNumber.Contains(searchTerm) ||
                    (workLog.Asset.PartNumber != null &&
                     workLog.Asset.PartNumber.Contains(searchTerm)) ||
                    (workLog.WorkOrderNumber != null &&
                     workLog.WorkOrderNumber.Contains(searchTerm)) ||
                    workLog.Issue.Contains(searchTerm) ||
                    workLog.WorkDone.Contains(searchTerm));
            }

            if (assetId.HasValue)
            {
                query = query.Where(workLog =>
                    workLog.AssetId == assetId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(workLog =>
                    workLog.WorkDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(workLog =>
                    workLog.WorkDate < endDate.Value.Date.AddDays(1));
            }

            var workLogs = await query
                .OrderByDescending(workLog => workLog.WorkDate)
                .ThenByDescending(workLog => workLog.CreatedAt)
                .ToListAsync();

            var userIds = workLogs
                .SelectMany(workLog => workLog.LaborEntries)
                .Select(entry => entry.UserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.UserName ?? user.Email ?? "Unknown user");

            var viewModel = new WorkLogIndexViewModel
            {
                SearchTerm = searchTerm,
                AssetId = assetId,
                StartDate = startDate,
                EndDate = endDate,

                AssetOptions = await _context.Assets
                    .AsNoTracking()
                    .OrderBy(asset => asset.AssetNumber)
                    .Select(asset => new SelectListItem
                    {
                        Value = asset.Id.ToString(),
                        Text = asset.AssetNumber
                    })
                    .ToListAsync(),

                WorkLogs = workLogs.Select(workLog =>
                    new WorkLogRowViewModel
                    {
                        Id = workLog.Id,
                        WorkDate = workLog.WorkDate,
                        AssetNumber = workLog.Asset.AssetNumber,
                        ProductNumber = workLog.Asset.PartNumber,
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
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(bool modal = false)
        {
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            var viewModel = new CreateWorkLogViewModel();

            viewModel.LaborEntries[0].UserId = currentUserId;

            await PopulateCreateOptionsAsync(viewModel);

            if (modal)
            {
                return PartialView(
                    "_CreateWorkLogForm",
                    viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateWorkLogViewModel viewModel,
            bool modal = false)
        {
            var currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            if (viewModel.AssetId.HasValue)
            {
                var assetExists = await _context.Assets
                    .AnyAsync(asset => asset.Id == viewModel.AssetId.Value);

                if (!assetExists)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.AssetId),
                        "The selected asset could not be found.");
                }
            }

            var completedLaborEntries = viewModel.LaborEntries
                .Where(entry =>
                    !string.IsNullOrWhiteSpace(entry.UserId) &&
                    entry.Hours.HasValue)
                .ToList();

            var incompleteLaborEntries = viewModel.LaborEntries
                .Any(entry =>
                    string.IsNullOrWhiteSpace(entry.UserId) !=
                    !entry.Hours.HasValue);

            if (incompleteLaborEntries)
            {
                ModelState.AddModelError(
                    nameof(viewModel.LaborEntries),
                    "Each labor entry must include both an employee and hours.");
            }

            if (completedLaborEntries.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(viewModel.LaborEntries),
                    "Add at least one labor entry.");
            }

            var incompleteDetailEntries = viewModel.DetailsUsed
                .Any(detail =>
                    string.IsNullOrWhiteSpace(detail.DetailNumber) &&
                    !string.IsNullOrWhiteSpace(detail.Description));

            if (incompleteDetailEntries)
            {
                ModelState.AddModelError(
                    nameof(viewModel.DetailsUsed),
                    "Each detail entry must include a detail number.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCreateOptionsAsync(viewModel);

                if (modal)
                {
                    return PartialView(
                        "_CreateWorkLogForm",
                        viewModel);
                }

                return View(viewModel);
            }

            var workLog = new WorkLog
            {
                WorkDate = viewModel.WorkDate,
                AssetId = viewModel.AssetId!.Value,
                WorkOrderNumber =
                    viewModel.WorkOrderNumber?.Trim(),
                Issue = viewModel.Issue.Trim(),
                WorkDone = viewModel.WorkDone.Trim(),
                Notes = viewModel.Notes?.Trim(),
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,

                LaborEntries = completedLaborEntries
                    .Select(entry => new LaborEntry
                    {
                        UserId = entry.UserId!,
                        Hours = entry.Hours!.Value
                    })
                    .ToList(),

                DetailsUsed = viewModel.DetailsUsed
                    .Where(detail =>
                        !string.IsNullOrWhiteSpace(detail.DetailNumber))
                    .Select(detail => new DetailUsage
                    {
                        DetailNumber = detail.DetailNumber!.Trim(),
                        Description = detail.Description?.Trim(),
                        Quantity = detail.Quantity
                    })
                    .ToList()
            };

            _context.WorkLogs.Add(workLog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Work log for {workLog.WorkDate:MM/dd/yyyy} was created.";

            if (modal)
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action(nameof(Index))
                });
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCreateOptionsAsync(
            CreateWorkLogViewModel viewModel)
        {
            var assets = await _context.Assets
                .AsNoTracking()
                .OrderBy(asset => asset.AssetNumber)
                .ToListAsync();

            viewModel.AssetOptions = assets.Select(asset =>
                new SelectListItem
                {
                    Value = asset.Id.ToString(),

                    Text = string.IsNullOrWhiteSpace(asset.PartNumber)
                        ? asset.AssetNumber
                        : $"{asset.AssetNumber} — {asset.PartNumber}",

                    Selected = viewModel.AssetId == asset.Id
                })
                .ToList();

            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(user => user.UserName)
                .ToListAsync();

            viewModel.EmployeeOptions = users.Select(user =>
                new SelectListItem
                {
                    Value = user.Id,

                    Text = user.UserName
                        ?? user.Email
                        ?? "Unnamed user"
                })
                .ToList();
        }
    }
}