using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("YMT_Programs")] // กำหนดชื่อตารางในฐานข้อมูล
    public class Programes
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int ProgramId { get; set; }
        public string? ProgramName { get; set; }

        public ICollection<UserAccess> Access { get; set; }
    }
}
