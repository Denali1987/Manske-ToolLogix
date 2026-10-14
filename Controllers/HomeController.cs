using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToolLogix.Data;
using ToolLogix.Models;

namespace ToolLogix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await GetAnnouncementsAsync();
            return View(announcements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(
            [Bind("Title,Message,Type,IsPinned")]
            ShopAnnouncement announcement)
        {
            if (!ModelState.IsValid)
            {
                var announcements = await GetAnnouncementsAsync();
                return View("Index", announcements);
            }

            announcement.AuthorName =
                User.Identity?.Name ?? "ToolLogix User";

            announcement.CreatedAtUtc = DateTime.UtcNow;

            _context.ShopAnnouncements.Add(announcement);
            await _context.SaveChangesAsync();

            TempData["AnnouncementStatus"] =
                "Announcement posted.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Assets(string? assetNumber)
        {
            var viewModel = new AssetSearchViewModel
            {
                SearchTerm = assetNumber?.Trim() ?? string.Empty,
                HasSearched = !string.IsNullOrWhiteSpace(assetNumber)
            };

            if (viewModel.HasSearched)
            {
                string search = viewModel.SearchTerm;

                viewModel.Results = await _context.Assets
                    .AsNoTracking()
                    .Where(asset =>
                        asset.AssetNumber.Contains(search) ||
                        asset.Description.Contains(search) ||
                        (asset.PartNumber != null &&
                         asset.PartNumber.Contains(search)) ||
                        (asset.PressNumber != null &&
                         asset.PressNumber.Contains(search)))
                    .OrderBy(asset => asset.AssetNumber)
                    .Take(50)
                    .ToListAsync();
            }

            return View(viewModel);
        }

        public IActionResult WorkLogs()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }

        private Task<List<ShopAnnouncement>>
            GetAnnouncementsAsync()
        {
            return _context.ShopAnnouncements
                .AsNoTracking()
                .OrderByDescending(
                    announcement => announcement.IsPinned)
                .ThenByDescending(
                    announcement =>
                        announcement.Type ==
                        AnnouncementType.Priority)
                .ThenByDescending(
                    announcement =>
                        announcement.CreatedAtUtc)
                .ToListAsync();
        }
    }
}