namespace PO_Api.Data.DTO
{
    public class EmailTemplateDTO
    {
        public string subject { get; set; } = string.Empty;
        public string body { get; set; } = string.Empty;
        public string? link {  get; set; } = null;

        public string? btnName { get; set; } = string.Empty;

    }

}
