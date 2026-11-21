using System.Security.Claims;
using BiristaCRM.Data;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class AuthRepository : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserMaster?> ValidateLoginAsync(string email, string password)
        {
            var user = await _db.UserMasters
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive);

            if (user == null || !PasswordHasher.Verify(password, user.Password))
            {
                return null;
            }

            return user;
        }

        public async Task SignInAsync(UserMaster user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("CompanyId", user.CompanyId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Remember Me: persistent cookie that survives browser restarts (30 days, sliding).
            // Unchecked: session cookie that ends when the browser closes.
            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                AllowRefresh = true,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);
        }

        public async Task SignOutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _db.UserMasters.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null || !PasswordHasher.Verify(currentPassword, user.Password))
            {
                return false;
            }

            user.Password = PasswordHasher.Hash(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateProfileAsync(Guid userId, string fullName, string mobile)
        {
            var user = await _db.UserMasters.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null)
            {
                return;
            }

            user.FullName = fullName;
            user.Mobile = mobile;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;
            await _db.SaveChangesAsync();
        }

        public async Task<UserMaster?> GetUserByIdAsync(Guid userId)
        {
            return await _db.UserMasters.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }
    }
}
