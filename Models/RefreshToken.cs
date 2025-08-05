using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("YMT_RefreshToken")] // กำหนดชื่อตารางในฐานข้อมูล
    public class RefreshToken
    {

        [Key] // กำหนดให้ Id เป็น Primary Key
        [Required]
        public int id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }



    }
}
