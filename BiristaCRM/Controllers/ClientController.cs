using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ClientFilterViewModel filter)
        {
            filter ??= new ClientFilterViewModel();
            var clients = await _clientService.GetAllClients(filter);
            ViewBag.Filter = filter;
            ViewBag.Areas = await _clientService.GetDistinctAreasAsync();
            return View(clients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateClientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clientService.CreateClient(model);
                TempData["Success"] = "Customer created successfully.";
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
            var client = await _clientService.GetSingleClientDetails(id);
            if (client == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CreateClientViewModel
            {
                BusinessName = client.BusinessName,
                OwnerName = client.OwnerName,
                Email = client.Email,
                GSTNo = client.GSTNo,
                MobileNumber = client.MobileNumber,
                Area = client.Area,
                Address = client.Address,
                Status = client.Status,
                IsActive = client.IsActive
            };

            ViewBag.ClientId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CreateClientViewModel model)
        {
            ViewBag.ClientId = id;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clientService.UpdateClient(id, model);
                TempData["Success"] = "Customer updated successfully.";
                return RedirectToAction(nameof(GetSingleClientDetails), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSingleClientDetails(Guid id)
        {
            var details = await _clientService.GetClientDetailsWithVisits(id);
            if (details == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _clientService.SoftDeleteClient(id);
            TempData["Success"] = "Customer deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
