using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_Role")] // กำหนดชื่อตารางในฐานข้อมูล
    public class Role
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public ICollection<User>? Users { get; set; } // ความสัมพันธ์กับ User
    }
}
