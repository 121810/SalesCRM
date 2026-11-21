using BiristaCRM.Enums;

namespace BiristaCRM.Interfaces
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid UserId { get; }
        Guid CompanyId { get; }
        string FullName { get; }
        string Email { get; }
        UserRole Role { get; }
        bool IsSuperAdmin { get; }
        bool IsCompanyAdmin { get; }
        bool IsSalesManager { get; }
        bool IsSalesPerson { get; }
        bool CanManageCompany { get; }
        bool CanManageUsers { get; }
        bool CanManageProducts { get; }
    }
}
