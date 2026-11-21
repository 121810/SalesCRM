namespace BiristaCRM.Models
{
    public class ProductMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? ProductCode { get; set; }

        public string? Description { get; set; }

        public string? Unit { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
