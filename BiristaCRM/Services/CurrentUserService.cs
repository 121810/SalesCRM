using System.Security.Claims;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;

namespace BiristaCRM.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public Guid UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

        public Guid CompanyId => Guid.TryParse(User?.FindFirstValue("CompanyId"), out var id) ? id : Guid.Empty;

        public string FullName => User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public UserRole Role => Enum.TryParse<UserRole>(User?.FindFirstValue(ClaimTypes.Role), out var role) ? role : UserRole.SalesPerson;

        public bool IsSuperAdmin => Role == UserRole.SuperAdmin;

        public bool IsCompanyAdmin => Role == UserRole.Admin;

        public bool IsSalesManager => Role == UserRole.SalesManager;

        public bool IsSalesPerson => Role == UserRole.SalesPerson;

        public bool CanManageCompany => IsSuperAdmin || IsCompanyAdmin;

        public bool CanManageUsers => IsSuperAdmin || IsCompanyAdmin || IsSalesManager;

        public bool CanManageProducts => IsSuperAdmin || IsCompanyAdmin || IsSalesManager;
    }
}
