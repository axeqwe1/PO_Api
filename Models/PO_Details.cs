using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_Details")]
    public class PO_Details
    {
        [Key]
        public long TempId { get; set; }

        public string? PONo { get; set; }
        public string? MatrClass { get; set; }
        public string? MatrCode { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public DateTime? FinalETADate { get; set; }
        public string? Unit { get; set; }
        public decimal? Price { get; set; }
        public decimal Qty { get; set; } = 0;
        public decimal? TotalAmount { get; set; }
        public decimal? ChargeValue { get; set; }
        public decimal? ChargeAmt { get; set; }

        [ForeignKey("PONo")]
        public virtual PO_Main PO_Main { get; set; }

    }
}
