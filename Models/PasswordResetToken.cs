using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PO_Api.Models
{
    [Table("PO_PasswordResetToken")]
    public class PO_PasswordResetToken
    {
        [Key]
        [Required]
        public int? id { get; set; }
        public string? token { get; set; }
        public string? email { get; set; }
        public DateTime? expires { get; set; }
        public DateTime? create_at { get; set; } = DateTime.Now;

        public bool used { get; set; } = false;
        public int user_id { get; set; } // Foreign key to the user table 
    }
}
