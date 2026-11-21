using System.ComponentModel.DataAnnotations;
using BiristaCRM.Enums;

namespace BiristaCRM.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class CreateCompanyViewModel
    {
        [Required]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10 digit mobile number.")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(100)]
        [Display(Name = "Admin Full Name")]
        public string AdminFullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Admin Email")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters. Special characters like @ are allowed.")]
        [DataType(DataType.Password)]
        [Display(Name = "Admin Password")]
        public string AdminPassword { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10 digit Mobile Number.")]
        [Display(Name = "Admin Mobile")]
        public string AdminMobile { get; set; } = string.Empty;
    }

    public class UpdateCompanyViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int UserCount { get; set; }
        public int ClientCount { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public UserRole Role { get; set; } = UserRole.SalesPerson;

        [Display(Name = "Manager")]
        public Guid? ManagerId { get; set; }

        [Display(Name = "Company")]
        public Guid? CompanyId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class CreateProductViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Product Code")]
        public string? ProductCode { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public class CreateVisitViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [Display(Name = "Client")]
        public Guid ClientId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public Guid ProductId { get; set; }

        [Required]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Status")]
        public VisitStatus Status { get; set; } = VisitStatus.New;

        [Display(Name = "Sample Given")]
        public bool IsSampleGiven { get; set; }

        [Display(Name = "Sample Quantity")]
        public decimal? SampleQuantity { get; set; }

        [Display(Name = "Current Buying Price")]
        public decimal? CurrentBuyingPrice { get; set; }

        [Display(Name = "Offer Price")]
        public decimal? OfferPrice { get; set; }

        [Display(Name = "Monthly Requirement")]
        public decimal? MonthlyRequirement { get; set; }

        [Display(Name = "Next Follow-Up Date")]
        public DateTime? NextFollowUpDate { get; set; }

        [Required]
        [Display(Name = "Customer Response")]
        public CustomerResponse CustomerResponse { get; set; } = CustomerResponse.Interested;

        [Display(Name = "Remark")]
        public string? Remark { get; set; }
    }

    public class CreateOrderViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [Display(Name = "Client")]
        public Guid ClientId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public Guid ProductId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Required]
        [Display(Name = "Payment Mode")]
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;

        [Required]
        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Remark")]
        public string? Remark { get; set; }
    }

    public class VisitListItemViewModel
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SalesPersonName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public VisitStatus Status { get; set; }
        public CustomerResponse CustomerResponse { get; set; }
        public bool IsSampleGiven { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public string? Remark { get; set; }
    }

    public class OrderListItemViewModel
    {
        public Guid Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string SalesPersonName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalClients { get; set; }
        public int ClientsThisMonth { get; set; }
        public int TodaysVisits { get; set; }
        public int VisitsInProgress { get; set; }
        public int OpenOrders { get; set; }
        public int OrdersToday { get; set; }
        public decimal RevenueMtd { get; set; }
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartValues { get; set; } = new();
        public List<ActivityItemViewModel> RecentActivity { get; set; } = new();
        public List<FollowUpItemViewModel> TodaysFollowUps { get; set; } = new();
        public List<OrderListItemViewModel> RecentOrders { get; set; } = new();
    }

    public class ActivityItemViewModel
    {
        public string Text { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
    }

    public class FollowUpItemViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = "sf-badge-warning";
    }

    public class ReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public decimal ConversionRate { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal VisitCompletionRate { get; set; }
        public int NewClientsThisMonth { get; set; }
        public int TotalVisits { get; set; }
        public int TotalOrders { get; set; }
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartValues { get; set; } = new();
        public List<PerformerViewModel> TopPerformers { get; set; } = new();
        public List<(Guid Id, string Name)> SalesPeople { get; set; } = new();
    }

    public class PerformerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int VisitCount { get; set; }
        public int OrderCount { get; set; }
        public int Rank { get; set; }
    }

    public class SettingsViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{10}$")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }

    public class CompanyIndexViewModel
    {
        public bool IsSuperAdmin { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public List<CompanyListItemViewModel> Companies { get; set; } = new();
        public UpdateCompanyViewModel? CurrentCompany { get; set; }
        public CreateCompanyViewModel CreateForm { get; set; } = new();
    }

    public class CompanyListItemViewModel
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClientDetailsViewModel
    {
        public Models.ClientMaster Client { get; set; } = null!;
        public List<VisitListItemViewModel> RecentVisits { get; set; } = new();
    }
}
