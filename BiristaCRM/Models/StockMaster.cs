using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiristaCRM.Models
{
    public class StockMaster : BaseEntity
    {
        public Guid CompanyId { get; set; }

        public Guid? ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ProductSubType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Qty { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(150)]
        public string ProductSender { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FactoryToTransportCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TransportToCompanyCost { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotalCost { get; set; } = 0;

        [StringLength(50)]
        public string? VehicleType { get; set; }

        [StringLength(50)]
        public string? VehicleNo { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }
    }
}
