using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    public class LoginController : Controller
    {
        private const string RememberEmailCookie = "sf_remember_email";
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new LoginViewModel();
            if (Request.Cookies.TryGetValue(RememberEmailCookie, out var savedEmail)
                && !string.IsNullOrWhiteSpace(savedEmail))
            {
                model.Email = savedEmail;
                model.RememberMe = true;
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.ValidateLoginAsync(model.Email.Trim(), model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            await _authService.SignInAsync(user, model.RememberMe);
            SetRememberEmailCookie(model.RememberMe ? model.Email.Trim() : null);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutGet()
        {
            await _authService.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        private void SetRememberEmailCookie(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Response.Cookies.Delete(RememberEmailCookie);
                return;
            }

            Response.Cookies.Append(RememberEmailCookie, email, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }
    }
}
