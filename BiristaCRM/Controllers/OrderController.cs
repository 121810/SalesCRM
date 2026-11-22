using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IClientService _clientService;
        private readonly IProductService _productService;
        private readonly ICurrentUserService _current;

        public OrderController(
            IOrderService orderService,
            IClientService clientService,
            IProductService productService,
            ICurrentUserService current)
        {
            _orderService = orderService;
            _clientService = clientService;
            _productService = productService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index(OrderFilterViewModel filter)
        {
            filter ??= new OrderFilterViewModel();
            var orders = await _orderService.GetOrdersAsync(filter);
            var stats = await _orderService.GetStatsAsync();
            ViewBag.OrderCount = stats.Count;
            ViewBag.Revenue = stats.Revenue;
            ViewBag.AwaitingPayment = stats.AwaitingPayment;
            ViewBag.Filter = filter;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            await PopulateLookups();
            return View(new CreateOrderViewModel { OrderDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookups();
                return View(model);
            }

            try
            {
                await _orderService.CreateOrderAsync(model);
                TempData["Success"] = "Order created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLookups();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateLookups();
            return View(new CreateOrderViewModel
            {
                Id = order.Id,
                ClientId = order.ClientId,
                ProductId = order.ProductId,
                OrderDate = order.OrderDate.ToLocalTime().Date,
                Quantity = order.Quantity,
                Rate = order.Rate,
                PaymentMode = order.PaymentMode,
                PaymentStatus = order.PaymentStatus,
                PaymentDate = order.PaymentDate?.ToLocalTime().Date,
                Remark = order.Remark
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookups();
                return View(model);
            }

            try
            {
                await _orderService.UpdateOrderAsync(id, model);
                TempData["Success"] = "Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLookups();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _orderService.SoftDeleteOrderAsync(id);
            TempData["Success"] = "Order deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLookups()
        {
            var clients = await _clientService.GetAllClients();
            var products = await _productService.GetProductsAsync();
            ViewBag.Clients = new SelectList(clients.Where(c => c.IsActive), "Id", "BusinessName");
            ViewBag.Products = products.Where(p => p.IsActive).Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(p.Unit) ? p.ProductName : $"{p.ProductName} ({p.Unit})"
            }).ToList();

            ViewBag.ProductData = products.Where(p => p.IsActive).Select(p => new
            {
                id = p.Id.ToString(),
                name = p.ProductName,
                price = p.Price,
                unit = p.Unit ?? ""
            }).ToList();
        }
    }
}
