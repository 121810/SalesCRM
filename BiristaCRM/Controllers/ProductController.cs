using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICurrentUserService _current;

        public ProductController(IProductService productService, ICurrentUserService current)
        {
            _productService = productService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            filter ??= new ProductFilterViewModel();
            var products = await _productService.GetProductsAsync(filter);
            ViewBag.Filter = filter;
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!_current.CanManageProducts || _current.IsSuperAdmin)
            {
                return Forbid();
            }

            return View(new CreateProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (!_current.CanManageProducts || _current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _productService.CreateProductAsync(model);
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!_current.CanManageProducts || _current.IsSuperAdmin)
            {
                return Forbid();
            }

            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateProductViewModel
            {
                Id = product.Id,
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                Description = product.Description,
                Unit = product.Unit,
                Price = product.Price,
                IsActive = product.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateProductViewModel model)
        {
            if (!_current.CanManageProducts || _current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _productService.UpdateProductAsync(id, model);
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_current.CanManageProducts || _current.IsSuperAdmin)
            {
                return Forbid();
            }

            await _productService.SoftDeleteProductAsync(id);
            TempData["Success"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
