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

            var viewModel = new AssetDetailsViewModel
            {
                Asset = asset,

                // Temporary test print.
                PrintUrl =
                    "https://www.diedesigner.com/uploads/5/0/8/1/50813613/formdie.pdf"
            };

            return View(viewModel);
        }
    }
}