using PO_Api.Data.DTO;
using PO_Api.Data.DTO.Response;

namespace PO_Api.Services
{
    public interface IFileService
    {
        Task<FileUploadResponse> UploadFilesAsync(string poNo, int uploadType, List<IFormFile> files);
        Task<List<FileUploadDto>> GetFilesByPONoAsync(string poNo, int uploadType);
        Task<bool> DeleteFileAsync(int fileId);
        Task<FileStream> DownloadFileAsync(int fileId);
        Task<FileUploadDto> GetFileByIdAsync(int fileId);
        Task<bool> UpdateDescriptionAsync(int fileId, string description);
    }
}
