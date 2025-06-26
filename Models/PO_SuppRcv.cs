using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_SuppRcv")]
    public class PO_SuppRcv
    {
        [Key]
        [Required]
        public string? PONo { get; set; }
        public bool? SuppRcvPO { get; set; }
        public DateTime? SuppRcvDate { get; set; } = DateTime.Now;
        public int SuppCancelPO { get; set; }
        public DateTime? SuppCancelDate { get; set; }
        public string? Remark { get; set; }
        public string? ConfirmCancelBy { get; set; }
        public DateTime? ConfirmCancelDate { get; set; }
    }
}
