using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourProject.Models;

namespace PO_Api.Models
{
    [Table("PO_Main")]
    public class PO_Main
    {

        [Key]
        [Required]
        public string? PONo { get; set; }
        public string? CompanyCode { get; set; }
        public string? SuppCode { get; set; }
        public string? SuppContact { get; set; }
        public bool? ClosePO { get; set; }
        public bool? CancelPO { get; set; }
        public bool? POReady { get; set; }
        public DateTime? ApproveDate { get; set; }
        public DateTime? FinalETADate { get; set; }
        public decimal? AmountNoVat { get; set; }
        public decimal? AmountTotal { get; set; }
        public decimal? TotalVat { get; set; }
        public decimal? TotalChange { get; set; }
        [ForeignKey("SuppCode")]
        public virtual Supplier? Suppliers { get; set; }
        public virtual ICollection<PO_Details>? Details { get; set; }

        [ForeignKey("PONo")]
        public virtual PO_SuppRcv? ReceiveInfo { get; set; }
        public virtual ICollection<PO_FileAttachment>? FileAttachment { get; set; }
    }
}
