using BiristaCRM.Enums;

namespace BiristaCRM.Models
{
    public class OrderMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public Guid ClientId { get; set; }

        public Guid ProductId { get; set; }

        public Guid SalesPersonId { get; set; }

        public string OrderNo { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public PaymentMode PaymentMode { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }

        public string? Remark { get; set; }
    }
}
