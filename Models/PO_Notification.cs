// Models/PO_FileAttachment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    [Table("PO_Notifications")]
    public class PO_Notifications
    {
        [Key]
        public Guid noti_id { get; set; }

        [Required]
        public string title { get; set; } = string.Empty;

        public string message { get; set; } = string.Empty;

        public string type { get; set; } = string.Empty;

        public string refId { get; set; } = string.Empty;
        public string? refType { get; set; } = string.Empty;
        public DateTime createAt { get; set; } = DateTime.UtcNow;
        public string createBy { get; set; } = string.Empty;

        public ICollection<PO_NotificationReceiver> Receivers { get; set; } = new List<PO_NotificationReceiver>();

    }
}