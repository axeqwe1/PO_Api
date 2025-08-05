using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("Monitor_LineStatusDetail")]
    public class WorkMinuteModel
    {
        [Key]
        public string line { get; set; } = string.Empty;
        public DateTime DateQc { get; set; }
        public decimal Minute { get; set; }
        public double MinuteDay { get; set; }
        public int QtyQC { get; set; }

        public virtual MasterLine MasterLine { get; set; }

    }
}
