using BiristaCRM.Enums;

namespace BiristaCRM.Models
{
    public class ClientMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public Guid SalesPersonId { get; set; }

        public string BusinessName { get; set; } = string.Empty;
        public string? Email { get; set; }

        public string? GSTNo { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public ClientStatus Status { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
