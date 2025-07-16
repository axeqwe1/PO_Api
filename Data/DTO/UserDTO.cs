using PO_Api.Models;

namespace PO_Api.Data.DTO
{
    public class UserDTO
    {
        public int userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string? supplierId { get; set; } = null ;
        public int roleId { get; set; } = 0; // 0 = Admin, 1 = Supplier, 2 = Customer
        public string supplierName { get; set; } = string.Empty;
        public string roleName { get; set; } = string.Empty;

    }

}
