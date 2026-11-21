using BiristaCRM.Enums;

namespace BiristaCRM.Models
{
    public class VisitMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public Guid ClientId { get; set; }

        public Guid ProductId { get; set; }

        public Guid SalesPersonId { get; set; }

        public DateTime VisitDate { get; set; }

        public VisitStatus Status { get; set; }

        public bool IsSampleGiven { get; set; }

        public decimal? SampleQuantity { get; set; }

        public decimal? CurrentBuyingPrice { get; set; }

        public decimal? OfferPrice { get; set; }

        public decimal? MonthlyRequirement { get; set; }

        public DateTime? NextFollowUpDate { get; set; }

        public CustomerResponse CustomerResponse { get; set; }

        public string? Remark { get; set; }
    }
}
