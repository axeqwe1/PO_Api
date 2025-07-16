using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_UserEmail")] // กำหนดชื่อตารางในฐานข้อมูล
    public class UserEmail
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public Guid emailId { get; set; }
        [Required]
        public int userId { get; set; }

        [Required]
        public string email {  get; set; }
        [Required]
        public bool isActive {  get; set; }

        public virtual User User { get; set; }
    }
}
