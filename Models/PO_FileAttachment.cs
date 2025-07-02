// Models/PO_FileAttachment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProject.Models
{
    [Table("PO_FileAttachment")]
    public class PO_FileAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Filename { get; set; } = string.Empty;

        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public DateTime UploadDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Url { get; set; }

        [Required]
        [StringLength(50)]
        public string PONo { get; set; } = string.Empty;

        [StringLength(255)]
        public string OriginalName { get; set; } = string.Empty;

        [Required]
        public byte UploadByType { get; set; }

        public int FileSize { get; set; }
        public string? Remark { get; set; } = string.Empty;
    }
}