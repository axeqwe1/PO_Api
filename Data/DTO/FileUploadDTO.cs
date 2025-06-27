namespace PO_Api.Data.DTO
{
    public class FileUploadDto
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string Type { get; set; }
        public DateTime UploadDate { get; set; }
        public string Url { get; set; }
        public string PONo { get; set; }
        public string OriginalName { get; set; }
        public long FileSize { get; set; }
    }

}
