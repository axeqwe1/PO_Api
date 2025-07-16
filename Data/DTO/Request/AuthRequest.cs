namespace PO_Api.Data.DTO.Request
{
    public class ForgetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
