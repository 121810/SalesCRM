using BiristaCRM.Enums;

namespace BiristaCRM.Models
{
    public class UserMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public Guid? ManagerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
