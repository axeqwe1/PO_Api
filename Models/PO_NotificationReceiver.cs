// Models/PO_FileAttachment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    [Table("PO_NotificationReceivers")]
    public class PO_NotificationReceiver
    {
        [Key]
        public Guid noti_recvId { get; set; }

        public Guid noti_id { get; set; } 

        public int userId { get; set; }

        
        public bool isRead { get; set; } = false;

        public DateTime? readAt { get; set; } 
        public bool isArchived { get; set; } 

        public virtual PO_Notifications Notification { get; set; } = null!;
    }
}