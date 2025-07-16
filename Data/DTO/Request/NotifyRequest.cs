namespace PO_Api.Data.DTO.Request
{
    public class NotifyRequest
    {
        public string Message { get; set; }
        public string PONo { get; set; } // Optional, if you want to send to a specific user
        

    }
}
