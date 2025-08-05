using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{

    public class SupMonitoring
    {
        public string? NameGroup { get; set; }
        public string? Super { get; set; }
        public string? GroupLineName { get; set; }
        public decimal? Targetpercent { get; set; }
        public int? Target {  get; set; }
        public int? Actual {  get; set; }
        public int? SupDf { get; set; } = 0;
        public decimal? Eff { get; set; } = 0;
        public decimal? PercentDefect { get; set; } = 0;
        public double? Effs { get; set; } = 0;

    }
}
