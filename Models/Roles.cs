using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("YMT_Roles")] // กำหนดชื่อตารางในฐานข้อมูล
    public class Roles
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public ICollection<UserAccess> Access { get; set; }

    }
}
