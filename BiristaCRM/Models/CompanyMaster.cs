namespace BiristaCRM.Models
{
    public class CompanyMaster : BaseEntity
    {
        public string CompanyName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
        public string? Logo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
