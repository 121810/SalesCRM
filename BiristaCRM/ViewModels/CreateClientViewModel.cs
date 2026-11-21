using BiristaCRM.Enums;
using System.ComponentModel.DataAnnotations;

namespace BiristaCRM.ViewModels
{
    public class CreateClientViewModel
    {
        [Required(ErrorMessage = "Business Name is required.")]
        [StringLength(100)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Owner Name is required.")]
        [StringLength(100)]
        [Display(Name = "Owner Name")]
        public string OwnerName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "GST Number")]
        public string? GSTNo { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Enter valid 10 digit Mobile Number.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required.")]
        [Display(Name = "Area")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public ClientStatus Status { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
