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
    public class SpareInventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpareInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? searchTerm,
            int? assetId,
            string? stockStatus)
        {
            var query = _context.SpareItems
                .AsNoTracking()
                .Include(item => item.Asset)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(item =>
                    item.DetailNumber.Contains(searchTerm) ||
                    (item.Description != null &&
                     item.Description.Contains(searchTerm)) ||
                    (item.StorageLocation != null &&
                     item.StorageLocation.Contains(searchTerm)));
            }

            if (assetId.HasValue)
            {
                query = query.Where(item =>
                    item.AssetId == assetId.Value);
            }

            switch (stockStatus)
            {
                case "In Stock":
                    query = query.Where(item =>
                        item.QuantityOnHand > item.MinimumQuantity);
                    break;

                case "Low Stock":
                    query = query.Where(item =>
                        item.QuantityOnHand > 0 &&
                        item.QuantityOnHand <= item.MinimumQuantity);
                    break;

                case "Out of Stock":
                    query = query.Where(item =>
                        item.QuantityOnHand == 0);
                    break;
            }

            var spareItems = await query
                .OrderBy(item =>
                    item.QuantityOnHand == 0
                        ? 0
                        : item.QuantityOnHand <= item.MinimumQuantity
                            ? 1
                            : 2)
                .ThenBy(item => item.DetailNumber)
                .Select(item => new SpareInventoryRowViewModel
                {
                    Id = item.Id,
                    DetailNumber = item.DetailNumber,
                    Description = item.Description,
                    AssetId = item.AssetId,

                    AssetNumber = item.Asset == null
                        ? null
                        : item.Asset.AssetNumber,

                    StorageLocation = item.StorageLocation,
                    QuantityOnHand = item.QuantityOnHand,
                    MinimumQuantity = item.MinimumQuantity,

                    StockStatus = item.QuantityOnHand == 0
                        ? "Out of Stock"
                        : item.QuantityOnHand <= item.MinimumQuantity
                            ? "Low Stock"
                            : "In Stock",

                    LastUpdatedAt = item.LastUpdatedAt
                })
                .ToListAsync();

            var viewModel = new SpareInventoryIndexViewModel
            {
                SearchTerm = searchTerm,
                AssetId = assetId,
                StockStatus = stockStatus,
                SpareItems = spareItems,

                AssetOptions = await _context.Assets
                    .AsNoTracking()
                    .OrderBy(asset => asset.AssetNumber)
                    .Select(asset => new SelectListItem
                    {
                        Value = asset.Id.ToString(),
                        Text = asset.AssetNumber
                    })
                    .ToListAsync(),

                TotalItemCount = await _context.SpareItems
                    .AsNoTracking()
                    .CountAsync(),

                LowStockCount = await _context.SpareItems
                    .AsNoTracking()
                    .CountAsync(item =>
                        item.QuantityOnHand > 0 &&
                        item.QuantityOnHand <= item.MinimumQuantity),

                OutOfStockCount = await _context.SpareItems
                    .AsNoTracking()
                    .CountAsync(item =>
                        item.QuantityOnHand == 0),

                TotalQuantityOnHand = await _context.SpareItems
                    .AsNoTracking()
                    .SumAsync(item => item.QuantityOnHand)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateSpareItemViewModel
            {
                AssetOptions = await GetAssetOptionsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateSpareItemViewModel viewModel)
        {
            var detailNumber = viewModel.DetailNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(detailNumber))
            {
                var duplicateExists = await _context.SpareItems
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.DetailNumber == detailNumber &&
                        item.AssetId == viewModel.AssetId);

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.DetailNumber),
                        "This detail number already exists for the selected asset.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.AssetOptions = await GetAssetOptionsAsync();
                return View(viewModel);
            }

            var spareItem = new SpareItem
            {
                DetailNumber = detailNumber!,
                Description = string.IsNullOrWhiteSpace(viewModel.Description)
                    ? null
                    : viewModel.Description.Trim(),

                AssetId = viewModel.AssetId,

                StorageLocation =
                    string.IsNullOrWhiteSpace(viewModel.StorageLocation)
                        ? null
                        : viewModel.StorageLocation.Trim(),

                QuantityOnHand = viewModel.QuantityOnHand,
                MinimumQuantity = viewModel.MinimumQuantity,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.SpareItems.Add(spareItem);
            await _context.SaveChangesAsync();

            TempData["InventoryMessage"] =
                $"Detail {spareItem.DetailNumber} was added to inventory.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var spareItem = await _context.SpareItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (spareItem == null)
            {
                return NotFound();
            }

            var viewModel = new EditSpareItemViewModel
            {
                Id = spareItem.Id,
                DetailNumber = spareItem.DetailNumber,
                Description = spareItem.Description,
                AssetId = spareItem.AssetId,
                StorageLocation = spareItem.StorageLocation,
                MinimumQuantity = spareItem.MinimumQuantity,
                AssetOptions = await GetAssetOptionsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditSpareItemViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var spareItem = await _context.SpareItems
                .FirstOrDefaultAsync(item => item.Id == id);

            if (spareItem == null)
            {
                return NotFound();
            }

            var detailNumber = viewModel.DetailNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(detailNumber))
            {
                var duplicateExists = await _context.SpareItems
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.Id != id &&
                        item.DetailNumber == detailNumber &&
                        item.AssetId == viewModel.AssetId);

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(viewModel.DetailNumber),
                        "This detail number already exists for the selected asset.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.AssetOptions = await GetAssetOptionsAsync();
                return View(viewModel);
            }

            spareItem.DetailNumber = detailNumber!;

            spareItem.Description =
                string.IsNullOrWhiteSpace(viewModel.Description)
                    ? null
                    : viewModel.Description.Trim();

            spareItem.AssetId = viewModel.AssetId;

            spareItem.StorageLocation =
                string.IsNullOrWhiteSpace(viewModel.StorageLocation)
                    ? null
                    : viewModel.StorageLocation.Trim();

            spareItem.MinimumQuantity = viewModel.MinimumQuantity;
            spareItem.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["InventoryMessage"] =
                $"Detail {spareItem.DetailNumber} was updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Adjust(int id)
        {
            var spareItem = await _context.SpareItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (spareItem == null)
            {
                return NotFound();
            }

            var viewModel = new AdjustSpareItemViewModel
            {
                Id = spareItem.Id,
                DetailNumber = spareItem.DetailNumber,
                Description = spareItem.Description,
                CurrentQuantity = spareItem.QuantityOnHand,
                AdjustmentType = "Add",
                Quantity = 1
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(
            int id,
            AdjustSpareItemViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var spareItem = await _context.SpareItems
                .FirstOrDefaultAsync(item => item.Id == id);

            if (spareItem == null)
            {
                return NotFound();
            }

            if ((viewModel.AdjustmentType == "Add" ||
                 viewModel.AdjustmentType == "Remove") &&
                viewModel.Quantity < 1)
            {
                ModelState.AddModelError(
                    nameof(viewModel.Quantity),
                    "Enter a quantity of at least 1.");
            }

            if (viewModel.AdjustmentType == "Remove" &&
                viewModel.Quantity > spareItem.QuantityOnHand)
            {
                ModelState.AddModelError(
                    nameof(viewModel.Quantity),
                    "The removed quantity cannot exceed the quantity on hand.");
            }

            if (viewModel.AdjustmentType == "Add" &&
                spareItem.QuantityOnHand + viewModel.Quantity > 100000)
            {
                ModelState.AddModelError(
                    nameof(viewModel.Quantity),
                    "The resulting quantity cannot exceed 100,000.");
            }

            if (viewModel.AdjustmentType != "Add" &&
                viewModel.AdjustmentType != "Remove" &&
                viewModel.AdjustmentType != "Set")
            {
                ModelState.AddModelError(
                    nameof(viewModel.AdjustmentType),
                    "Select a valid adjustment type.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.DetailNumber = spareItem.DetailNumber;
                viewModel.Description = spareItem.Description;
                viewModel.CurrentQuantity = spareItem.QuantityOnHand;

                return View(viewModel);
            }

            var adjustedByUserId =
    User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(adjustedByUserId))
            {
                return Challenge();
            }

            var previousQuantity = spareItem.QuantityOnHand;

            switch (viewModel.AdjustmentType)
            {
                case "Add":
                    spareItem.QuantityOnHand += viewModel.Quantity;
                    break;

                case "Remove":
                    spareItem.QuantityOnHand -= viewModel.Quantity;
                    break;

                case "Set":
                    spareItem.QuantityOnHand = viewModel.Quantity;
                    break;
            }

            spareItem.LastUpdatedAt = DateTime.UtcNow;

            var transaction = new InventoryTransaction
            {
                SpareItemId = spareItem.Id,
                AdjustmentType = viewModel.AdjustmentType,
                Quantity = viewModel.Quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = spareItem.QuantityOnHand,
                Reason = viewModel.Reason.Trim(),
                AdjustedByUserId = adjustedByUserId,
                AdjustedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            TempData["InventoryMessage"] =
                $"Detail {spareItem.DetailNumber} now has " +
                $"{spareItem.QuantityOnHand} on hand.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> History(int id)
        {
            var spareItem = await _context.SpareItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (spareItem == null)
            {
                return NotFound();
            }

            var transactions = await _context.InventoryTransactions
                .AsNoTracking()
                .Where(transaction => transaction.SpareItemId == id)
                .OrderByDescending(transaction => transaction.AdjustedAt)
                .ThenByDescending(transaction => transaction.Id)
                .ToListAsync();

            var userIds = transactions
                .Select(transaction => transaction.AdjustedByUserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.UserName ??
                            user.Email ??
                            "Unknown user");

            var viewModel = new InventoryHistoryViewModel
            {
                SpareItemId = spareItem.Id,
                DetailNumber = spareItem.DetailNumber,
                Description = spareItem.Description,
                CurrentQuantity = spareItem.QuantityOnHand,

                Transactions = transactions
                    .Select(transaction =>
                        new InventoryTransactionRowViewModel
                        {
                            Id = transaction.Id,
                            AdjustmentType = transaction.AdjustmentType,
                            Quantity = transaction.Quantity,
                            PreviousQuantity =
                                transaction.PreviousQuantity,
                            NewQuantity = transaction.NewQuantity,
                            Reason = transaction.Reason,

                            AdjustedByName = userNames.TryGetValue(
                                transaction.AdjustedByUserId,
                                out var adjustedByName)
                                    ? adjustedByName
                                    : "Unknown user",

                            AdjustedAt = transaction.AdjustedAt
                        })
                    .ToList()
            };

            return View(viewModel);
        }

        private async Task<List<SelectListItem>> GetAssetOptionsAsync()
        {
            return await _context.Assets
                .AsNoTracking()
                .OrderBy(asset => asset.AssetNumber)
                .Select(asset => new SelectListItem
                {
                    Value = asset.Id.ToString(),
                    Text = asset.AssetNumber
                })
                .ToListAsync();
        }

    }
}