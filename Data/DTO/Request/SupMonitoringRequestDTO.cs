namespace PO_Api.Data.DTO.Request
{
    public class SupMonitoringRequest
    {
        public DateTime date { get; set; } = DateTime.Now;
        public string mode { get; set; } = string.Empty;

        public string Company {  get; set; } = string.Empty;
    }
}
