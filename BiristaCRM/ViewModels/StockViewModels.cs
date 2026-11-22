using System.ComponentModel.DataAnnotations;

namespace BiristaCRM.ViewModels
{
    public class CreateStockViewModel
    {
        public Guid? Id { get; set; }

        [Display(Name = "Select Product (Optional)")]
        public Guid? ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Product SubType")]
        public string? ProductSubType { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Product Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Display(Name = "Quantity")]
        public decimal Qty { get; set; }

        public string? Unit { get; set; }

        [Display(Name = "Product Total Price")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Arrival Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Product Sender is required.")]
        [StringLength(150)]
        [Display(Name = "Product Sender (Company/Supplier)")]
        public string ProductSender { get; set; } = string.Empty;

        [Display(Name = "Factory to Transport Cost (₹)")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost cannot be negative.")]
        public decimal FactoryToTransportCost { get; set; } = 0;

        [Display(Name = "Transport to Company Cost (₹)")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost cannot be negative.")]
        public decimal TransportToCompanyCost { get; set; } = 0;

        [Display(Name = "Grand Total / Landed Cost (₹)")]
        public decimal GrandTotalCost { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Transport Vehicle (e.g. Truck, Bus, Tempo)")]
        public string? VehicleType { get; set; }

        [StringLength(50)]
        [Display(Name = "Vehicle Number (Optional)")]
        public string? VehicleNo { get; set; }

        [StringLength(500)]
        [Display(Name = "Remark / Bilty No / Notes")]
        public string? Remark { get; set; }
    }

    public class StockFilterViewModel
    {
        public string? Search { get; set; }
        public string? ProductSender { get; set; }
        public string? VehicleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class StockStatsViewModel
    {
        public int TotalEntries { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalTransportCost { get; set; }
        public decimal TotalLandedCost { get; set; }
    }
}
