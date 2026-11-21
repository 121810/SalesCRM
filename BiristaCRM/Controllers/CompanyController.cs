using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ICurrentUserService _current;

        public CompanyController(ICompanyService companyService, ICurrentUserService current)
        {
            _companyService = companyService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? isActive)
        {
            if (!_current.CanManageCompany && !_current.IsSuperAdmin)
            {
                return Forbid();
            }

            var model = await _companyService.GetCompanyPageAsync(search, isActive);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCompanyViewModel model)
        {
            if (!_current.IsSuperAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));

                TempData["Error"] = string.IsNullOrWhiteSpace(errors)
                    ? "Please fill all required fields correctly."
                    : errors;

                var page = await _companyService.GetCompanyPageAsync();
                page.CreateForm = model;
                return View("Index", page);
            }

            try
            {
                await _companyService.CreateCompanyWithAdminAsync(model);
                TempData["Success"] = "Company and admin account created successfully. Admin can sign in with the email and password you set.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var page = await _companyService.GetCompanyPageAsync();
                page.CreateForm = model;
                return View("Index", page);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateCompanyViewModel model)
        {
            if (!_current.CanManageCompany)
            {
                return Forbid();
            }

            try
            {
                await _companyService.UpdateCompanyAsync(model);
                TempData["Success"] = "Company updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_current.IsSuperAdmin)
            {
                return Forbid();
            }

            await _companyService.SoftDeleteCompanyAsync(id);
            TempData["Success"] = "Company deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
