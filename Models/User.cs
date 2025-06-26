using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_User")] // กำหนดชื่อตารางในฐานข้อมูล
    public class User
    {
        
        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int userId { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        [Required]
        public string? username { get; set; }
        [Required]
        public string? password { get; set; }
        public string? email { get; set; }
        public string? supplierId { get; set; }
        public int RoleId { get; set; } = 0; // 0 = Admin, 1 = Supplier, 2 = Customer

        public virtual Role? Role { get; set; } // ความสัมพันธ์กับ Role
    }
}
