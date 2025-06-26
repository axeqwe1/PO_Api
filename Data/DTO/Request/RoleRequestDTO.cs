namespace PO_Api.Data.DTO.Request
{
    public class RoleRequestDTO
    {
    }

    public class CreateRoleRequestDTO
    {
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
