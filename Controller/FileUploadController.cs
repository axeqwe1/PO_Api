using Microsoft.AspNetCore.Mvc;
using PO_Api.Data.DTO.Request;
using PO_Api.Services;
using YourProject.Models;

namespace PO_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IFileService fileService, ILogger<FileUploadController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFiles(FileUploadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.PONo))
                {
                    return BadRequest(new { success = false, message = "PO Number is required" });
                }

                if (request.Files == null || !request.Files.Any())
                {
                    return BadRequest(new { success = false, message = "No files provided" });
                }


                var result = await _fileService.UploadFilesAsync(request.PONo, request.UploadType, request.Files);
                

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in file upload endpoint");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet]
        [Route("files/{poNo}")]
        public async Task<IActionResult> GetFiles(string poNo, int uploadType)
        {
            try
            {
                var files = await _fileService.GetFilesByPONoAsync(poNo, uploadType);
                return Ok(new { success = true, files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving files for PO {poNo}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpDelete]
        [Route("delete/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var result = await _fileService.DeleteFileAsync(fileId);

                if (result)
                {
                    return Ok(new { success = true, message = "File deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "File not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {fileId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet]
        [Route("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var fileInfo = await _fileService.GetFileByIdAsync(fileId);
                if (fileInfo == null)
                {
                    
                    return StatusCode(404, new { success = false, message = "Not found file data in database" });
                }

                var fileStream = await _fileService.DownloadFileAsync(fileId);
                if (fileStream == null)
                {
                    return StatusCode(404, new { success = false, message = "Not found file data in directory maybe moved or deleted already" });
                }

                return File(fileStream, fileInfo.Type, fileInfo.OriginalName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {fileId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }


        [HttpPut]
        [Route("update-description/{fileId}")]
        public async Task<IActionResult> UpdateDescriptionFile(int fileId, [FromBody] DescriptionRequest request)
        {
            try
            {
                bool result = await _fileService.UpdateDescriptionAsync(fileId, request.Description);
                var message = "";
                if (result)
                {
                    message = "Description updated successfully";
                }
                else
                {
                    message = "Failed to update description or file not found";
                }
                return StatusCode(200, message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error updating description for file {fileId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
            
    }

    public class DeleteFileRequest
    {
        public string PONo { get; set; }
        public int uploadType
        {
            get; set;
        }
    }
    public class DescriptionRequest
    {
        public string Description { get; set; }
    }

}
