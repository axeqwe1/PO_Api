using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PO_Api.Models
{
    [Table("PO_Supplier")] // กำหนดชื่อตารางในฐานข้อมูล
    public class Supplier
    {
        
        [Key]
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? Country { get; set; }
        public string? Currency { get; set; }

    }
}
