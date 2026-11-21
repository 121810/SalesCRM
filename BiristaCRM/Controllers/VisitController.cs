using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class VisitController : Controller
    {
        private readonly IVisitService _visitService;
        private readonly IClientService _clientService;
        private readonly IProductService _productService;
        private readonly ICurrentUserService _current;

        public VisitController(
            IVisitService visitService,
            IClientService clientService,
            IProductService productService,
            ICurrentUserService current)
        {
            _visitService = visitService;
            _clientService = clientService;
            _productService = productService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index(VisitFilterViewModel filter)
        {
            filter ??= new VisitFilterViewModel();
            var visits = await _visitService.GetVisitsAsync(filter);
            var stats = await _visitService.GetStatsAsync();
            ViewBag.Today = stats.Today;
            ViewBag.Completed = stats.Completed;
            ViewBag.FollowUp = stats.FollowUp;
            ViewBag.New = stats.New;
            ViewBag.Filter = filter;
            return View(visits);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (_current.IsSuperAdmin)
            {
                return Forbid();
            }

            await PopulateLookups();
            return View(new CreateVisitViewModel { VisitDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVisitViewModel model)
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
                await _visitService.CreateVisitAsync(model);
                TempData["Success"] = "Visit logged successfully.";
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
            var visit = await _visitService.GetByIdAsync(id);
            if (visit == null)
            {
                TempData["Error"] = "Visit not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateLookups();
            return View(new CreateVisitViewModel
            {
                Id = visit.Id,
                ClientId = visit.ClientId,
                ProductId = visit.ProductId,
                VisitDate = visit.VisitDate.ToLocalTime(),
                Status = visit.Status,
                IsSampleGiven = visit.IsSampleGiven,
                SampleQuantity = visit.SampleQuantity,
                CurrentBuyingPrice = visit.CurrentBuyingPrice,
                OfferPrice = visit.OfferPrice,
                MonthlyRequirement = visit.MonthlyRequirement,
                NextFollowUpDate = visit.NextFollowUpDate?.ToLocalTime(),
                CustomerResponse = visit.CustomerResponse,
                Remark = visit.Remark
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateVisitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookups();
                return View(model);
            }

            try
            {
                await _visitService.UpdateVisitAsync(id, model);
                TempData["Success"] = "Visit updated successfully.";
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
            await _visitService.SoftDeleteVisitAsync(id);
            TempData["Success"] = "Visit deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLookups()
        {
            var clients = await _clientService.GetAllClients();
            var products = await _productService.GetProductsAsync();
            ViewBag.Clients = new SelectList(clients.Where(c => c.IsActive), "Id", "BusinessName");
            ViewBag.Products = new SelectList(products.Where(p => p.IsActive), "Id", "ProductName");
        }
    }
}
