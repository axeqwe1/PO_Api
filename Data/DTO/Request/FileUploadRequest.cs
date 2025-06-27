namespace PO_Api.Data.DTO.Request
{
    public class FileUploadRequest
    {
        public string PONo { get; set; } = string.Empty;

        public int UploadType { get; set; } = 0;
        public List<IFormFile> Files { get; set; }
    }
}
