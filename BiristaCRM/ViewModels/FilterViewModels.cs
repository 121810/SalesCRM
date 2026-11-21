using BiristaCRM.Enums;

namespace BiristaCRM.ViewModels
{
    public class ClientFilterViewModel
    {
        public string? Search { get; set; }
        public string? Area { get; set; }
        public ClientStatus? Status { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductFilterViewModel
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
    }

    public class VisitFilterViewModel
    {
        public string? Search { get; set; }
        public VisitStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class OrderFilterViewModel
    {
        public string? Search { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public PaymentMode? PaymentMode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class UserFilterViewModel
    {
        public string? Search { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? SalesPersonId { get; set; }
    }
}
