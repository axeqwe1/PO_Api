using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("Master_Line")]
    public class MasterLine
    {
        public int Id { get; set; }
        public string? Factory { get; set; }
        public string? Line { get; set; }
        public string? LineName { get; set; }
        public string? LineGroup { get; set; }
        public string NameGroup { get; set; } = string.Empty;
        public decimal? EffDay { get; set; }
        public bool? Cancel { get; set; }
        public int? SortByGroup { get; set; }
        public int? Worker { get; set; }
        public decimal? WorkHour { get; set; }
        public decimal? EffPerDay { get; set; }
        public string? Supervisor { get; set; }
        public bool? ShowInBuffer { get; set; }
        public string? FastreactId { get; set; }
        public bool? IsShift { get; set; }
        public bool? IsMask { get; set; }
        public bool? IsETON { get; set; }

        public ICollection<WorkMinuteModel> WorkMinute { get; set; }
        public ICollection<MonitorLineStatus> monitorLineStatuses { get; set; }
    }
}
