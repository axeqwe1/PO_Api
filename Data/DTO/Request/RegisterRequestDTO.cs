namespace PO_Api.Data.DTO.Request
{
    public class RegisterRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string[] Email { get; set; } = [];
        public string RoleName { get; set; } = "Admin"; // Default role is Customer
        public string SupplierId { get; set; } = string.Empty; // Optional, can be empty if not applicable
    }


    public class UpdateUserRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string[] Email { get; set; } = [];
        public string RoleName { get; set; } = string.Empty; // แนะนำใช้ RoleId แทน string RoleName
        public string SupplierId { get; set; } = string.Empty; // Optional, can be empty if not applicable
    }

    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = string.Empty; // Optional
        public string NewPassword { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string TokenResetPassword { get; set; } = string.Empty;
    }


}
