using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _current;

        public SettingsController(IAuthService authService, ICurrentUserService current)
        {
            _authService = authService;
            _current = current;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _authService.GetUserByIdAsync(_current.UserId);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(new SettingsViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role.ToString()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SettingsViewModel model)
        {
            model.Email = _current.Email;
            model.Role = _current.Role.ToString();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _authService.UpdateProfileAsync(_current.UserId, model.FullName, model.Mobile);

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required.");
                    return View(model);
                }

                var ok = await _authService.ChangePasswordAsync(_current.UserId, model.CurrentPassword, model.NewPassword);
                if (!ok)
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                    return View(model);
                }
            }

            TempData["Success"] = "Settings saved successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
