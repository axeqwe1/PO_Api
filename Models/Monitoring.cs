using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{

    public class Monitoring
    {
        public DateTime DateQC { get; set; }
        public string? Line { get; set; }
        public string? LineGroup { get; set; }
        public int? WorkMinute { get; set; }
        public int? WorkMinuteB {  get; set; }
        public bool? IsShift { get; set; }
        public double? EFFOrder { get; set; }

        public double? Eff { get; set; }

        public double? EffB { get; set; }
        public decimal? PerDefect { get; set; }
        public int? RunDay { get; set; }
        public string? WK { get; set; } = string.Empty;
        public string? OrderNo { get; set; } = string.Empty;
        public string? Style { get; set; } = string.Empty;
        public string? Customer { get; set; } = string.Empty;
        public string? ProgramCode { get; set; } = string.Empty;
        public string? Type { get; set; } = string.Empty;
        public DateTime? ShipDate { get; set; }
        public double? WorkDay { get; set; }
        [NotMapped]
        public double? Sam {  get; set; }
        [NotMapped]
        public double? SAMEst { get; set; }
        public double? WorkEff {  get; set; }
        public double? Total { get; set; }
        public int? TotalActual { get; set; }
        public int? TotalActualA { get; set; }
        public int? TotalActualB { get; set; }
        public int? ActHour0 { get; set; }
        public int? ActHour1 { get; set; }
        public int? ActHour2 { get; set; }
        public int? ActHour3 { get; set; }
        public int? ActHour4 { get; set; }
        public int? Total1 { get; set; }
        public int? ActHourNoon { get; set; }
        public int? ActHour5 { get; set; }
        public int? ActHour6 { get; set; }
        public int? ActHour7 { get; set; }
        public int? ActHour8 { get; set; }
        public int? Total2 { get; set; }
        public int? ActHour9 { get; set; }
        public int? ActHour10_1 { get; set; }
        public int? ActHour10 { get; set; }
        public int? ActHour11 { get; set; }
        public int? ActHour12 { get; set; }
        public int? ActHour13 { get; set; }
        public int? Total3 { get; set; }
        public int? ActHour14 { get; set; }
        public int? ActHour15 { get; set; }
        public int? ActHour16 { get; set; }
        public int? ActHour17 { get; set; }
        public int? ActHour18 { get; set; }
        public int? ActHour19 { get; set; }
        public int? ActHour20 { get; set; }
        public int? total4 { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;                         
        public decimal? WorkTotal { get; set; }
        public double? TargetHour { get; set; }

        public int? EffHour1 { get; set; } = 0;
        public int? EffHour2 { get; set; } = 0;
        public int? EffHour3 { get; set; } = 0;
        public int? EffHour4 { get; set; } = 0;
        public int? EffHour5 { get; set; } = 0; 
        public int? EffHour6 { get; set; } = 0;
        public int? EffHour7 { get; set; } = 0;
        public int? EffHour8 { get; set; } = 0;
        public int? EffHour9 { get; set; } = 0;
        public int? EffHour10 { get; set; } = 0;
        public int? EffHour11 { get; set; } = 0;
        public int? EffHour12 { get; set; } = 0;
        public int? EffHour13 { get; set; } = 0;
        public int? EffHour14 { get; set; } = 0;
        public int? EffHour15 { get; set; } = 0;
        public int? EffHour16 { get; set; } = 0;
        public int? EffHour17 { get; set; } = 0;
        public int? EffHour18 { get; set; } = 0;
        public int? EffHour19 { get; set; } = 0;
        public int? EffHour20 { get; set; } = 0;
        public int? TargetEFF { get; set; } = 0;
        public decimal? SAMPRO { get; set; } = 0;
        public double? SAMQU { get; set; } = 0;
        public double? Perzen { get; set; } = 0;
        public double? ACEFF { get; set; } = 0;
        public double? WorkACL { get; set; } = 0;
        public int? ActACL { get; set; } = 0;
        public double? SAMACL { get; set; } = 0;
        public double? IMEFF { get; set; } = 0;

    }
}
