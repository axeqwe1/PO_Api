using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_RefreshToken")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
