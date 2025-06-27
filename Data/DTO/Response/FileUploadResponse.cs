namespace PO_Api.Data.DTO.Response
{
    public class FileUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<FileUploadDto> UploadedFiles { get; set; }
    }
}
