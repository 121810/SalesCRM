using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ICurrentUserService _current;

        public StockController(
            IStockService stockService,
            IProductService productService,
            ICurrentUserService current)
        {
            _stockService = stockService;
            _productService = productService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index(StockFilterViewModel filter)
        {
            filter ??= new StockFilterViewModel();
            var stocks = await _stockService.GetStocksAsync(filter);
            var stats = await _stockService.GetStatsAsync(filter);
            ViewBag.Stats = stats;
            ViewBag.Filter = filter;
            ViewBag.Senders = await _stockService.GetDistinctSendersAsync();
            return View(stocks);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            await PopulateProductsLookup();
            return View(new CreateStockViewModel { Date = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStockViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await PopulateProductsLookup();
                return View(model);
            }

            try
            {
                await _stockService.CreateStockAsync(model);
                TempData["Success"] = "Stock entry recorded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateProductsLookup();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            var stock = await _stockService.GetByIdAsync(id);
            if (stock == null)
            {
                TempData["Error"] = "Stock entry not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateProductsLookup();
            var model = new CreateStockViewModel
            {
                Id = stock.Id,
                ProductId = stock.ProductId,
                ProductName = stock.ProductName,
                ProductSubType = stock.ProductSubType,
                Price = stock.Price,
                Qty = stock.Qty,
                Unit = stock.Unit,
                TotalPrice = stock.TotalPrice,
                Date = stock.Date.ToLocalTime().Date,
                ProductSender = stock.ProductSender,
                FactoryToTransportCost = stock.FactoryToTransportCost,
                TransportToCompanyCost = stock.TransportToCompanyCost,
                GrandTotalCost = stock.GrandTotalCost,
                VehicleType = stock.VehicleType,
                VehicleNo = stock.VehicleNo,
                Remark = stock.Remark
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateStockViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await PopulateProductsLookup();
                return View(model);
            }

            try
            {
                await _stockService.UpdateStockAsync(id, model);
                TempData["Success"] = "Stock entry updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateProductsLookup();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            await _stockService.SoftDeleteStockAsync(id);
            TempData["Success"] = "Stock entry deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateProductsLookup()
        {
            var products = await _productService.GetProductsAsync();
            ViewBag.Products = products.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(p.Unit) ? p.ProductName : $"{p.ProductName} ({p.Unit})"
            }).ToList();

            ViewBag.ProductData = products.Select(p => new
            {
                id = p.Id.ToString(),
                name = p.ProductName,
                price = p.Price,
                unit = p.Unit ?? ""
            }).ToList();
        }
    }
}
