using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("YMT_Users")] // กำหนดชื่อตารางในฐานข้อมูล
    public class User
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }

        public bool IsActive { get; set; } = false; // true = ลาออก, false = ยังทำงานอยู่

        public ICollection<UserAccess> Access { get; set; }

    }
}
