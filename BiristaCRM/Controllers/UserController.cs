using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _current;
        private readonly ICompanyService _companyService;

        public UserController(IUserService userService, ICurrentUserService current, ICompanyService companyService)
        {
            _userService = userService;
            _current = current;
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(UserFilterViewModel filter)
        {
            filter ??= new UserFilterViewModel();
            var users = await _userService.GetUsersAsync(filter);
            ViewBag.Filter = filter;
            await PopulateLookups();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!_current.CanManageUsers)
            {
                return Forbid();
            }

            await PopulateLookups();
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!_current.CanManageUsers)
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
                await _userService.CreateUserAsync(model);
                TempData["Success"] = _current.IsSuperAdmin ? "User created successfully." : "SalesPerson created successfully.";
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
        public async Task<IActionResult> ToggleActive(Guid id, bool isActive)
        {
            if (!_current.CanManageUsers)
            {
                return Forbid();
            }

            await _userService.SetActiveAsync(id, isActive);
            TempData["Success"] = isActive
                ? (_current.IsSuperAdmin ? "User activated." : "SalesPerson activated.")
                : (_current.IsSuperAdmin ? "User deactivated." : "SalesPerson deactivated.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_current.CanManageUsers)
            {
                return Forbid();
            }

            await _userService.SoftDeleteUserAsync(id);
            TempData["Success"] = _current.IsSuperAdmin ? "User deleted." : "SalesPerson deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLookups()
        {
            var managers = await _userService.GetManagersAsync();
            ViewBag.Managers = new SelectList(managers, "Id", "FullName");

            if (_current.IsSuperAdmin)
            {
                var companies = await _companyService.GetCompanyPageAsync();
                ViewBag.Companies = new SelectList(companies.Companies, "Id", "CompanyName");
            }

            var roles = new List<UserRole>();
            if (_current.IsSuperAdmin)
            {
                roles.Add(UserRole.Admin);
            }
            else if (_current.IsCompanyAdmin)
            {
                roles.Add(UserRole.SalesManager);
                roles.Add(UserRole.SalesPerson);
            }
            else if (_current.IsSalesManager)
            {
                roles.Add(UserRole.SalesPerson);
            }

            ViewBag.Roles = new SelectList(roles);
        }
    }
}
