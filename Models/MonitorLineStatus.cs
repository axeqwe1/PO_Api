using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("Monitor_LineStatus")]
    public class MonitorLineStatus
    {
        public DateTime DateQC { get; set; }

        [Key]
        public string Line { get; set; }

        public int? Worker { get; set; }
        public int? WorkerOT { get; set; }
        public int? Worker2 { get; set; }
        public int? Worker3 { get; set; }

        public string? OrderNo { get; set; }
        public string? Style { get; set; }

        public int? RunDay { get; set; }
        public int? LastMinute { get; set; }
        public int? SumMinute { get; set; }

        public double? TargetHour { get; set; }
        public double? ActualHour { get; set; }
        public double? ActualTotal { get; set; }
        public double? ActualAvg { get; set; }

        public int? QtyQC { get; set; }
        public int? QtyDefect { get; set; }
        public double? Efficiency { get; set; }

        public int? QtyWIP { get; set; }
        public double? WorkEffTarget { get; set; }

        public string? Comment { get; set; }
        public string? Description { get; set; }

        public double? WorkHour { get; set; }
        public double? WorkOT { get; set; }

        public double? SAMQU { get; set; }
        public double? ACTSAM { get; set; }

        public virtual MasterLine MasterLine { get; set; }
    }
}
