using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("YMT_UserAccess")] // กำหนดชื่อตารางในฐานข้อมูล
    public class UserAccess
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int AccessId { get; set; }
        public int? UserId { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ProgramId { get; set; }
        public int? RoleId { get; set; } // true = ลาออก, false = ยังทำงานอยู่

        public virtual User User { get; set; }
        public virtual Roles Roles { get; set; }
        public virtual Programes Programs { get; set; }


    }
}
