//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using PO_Api.Data;
//using PO_Api.Data.DTO;
//using PO_Api.Data.DTO.Response;
//using PO_Api.Hubs;
//using PO_Api.Models;


//namespace PO_Api.Services
//{
//    public class FileService : IFileService
//    {
//        private readonly AppDbContext _context;
//        private readonly IWebHostEnvironment _environment;
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<FileService> _logger;
//        private readonly string _networkPath = @"\\192.168.9.3\YMTMainFile\YPTMAIN\POMaterialYPT\POWebsite\"; // Update with your network path
//        private readonly string _localBasePath;
//        private readonly IHubContext<NotificationHub> _hub;

//        public FileService(
//            AppDbContext context,
//            IWebHostEnvironment environment,
//            IConfiguration configuration,
//            ILogger<FileService> logger,
//            IHubContext<NotificationHub> hub)
//        {
//            _context = context;
//            _environment = environment;
//            _configuration = configuration;
//            _logger = logger;
//            _localBasePath = Path.Combine(environment.ContentRootPath, "Uploads");
//            _hub = hub;

//        }

//        public async Task<FileUploadResponse> UploadFilesAsync(string poNo, int uploadType, List<IFormFile> files)
//        {
//            var response = new FileUploadResponse
//            {
//                UploadedFiles = new List<FileUploadDto>()
//            };

//            try
//            {
//                // Validate PO Number
//                if (string.IsNullOrEmpty(poNo))
//                {
//                    response.Success = false;
//                    response.Message = "PO Number is required";
//                    return response;
//                }

//                // Validate files
//                if (files == null || !files.Any())
//                {
//                    response.Success = false;
//                    response.Message = "No files provided";
//                    return response;
//                }

//                // ตรวจสอบก่อนใช้ และสร้างโฟลเดอร์สำรองหากไม่มี
//                string folderType = "";
//                if (uploadType == 1)
//                {
//                    folderType = "POSupplierFile";
//                }
//                if (uploadType == 2)
//                {
//                    folderType = "POPurchaseOfficerFile";
//                }

//                var networkPath = Path.Combine(_localBasePath, folderType);
//                if (!Directory.Exists(networkPath))
//                {
//                    Directory.CreateDirectory(networkPath);
//                }
//                // Create upload directory
//                var uploadPath = Path.Combine(networkPath, poNo);
//                if (!Directory.Exists(uploadPath))
//                {
//                    Directory.CreateDirectory(uploadPath);
//                }

//                var FileInDb = _context.PO_FileAttachments
//                    .Where(f => f.PONo == poNo && f.UploadByType == uploadType)
//                    .Select(f => new
//                    {
//                        f.PONo,
//                        FileSize = (long)f.FileSize // Cast ให้ชัดเจนตอนดึง
//                    })
//                    .ToList();
//                var total = FileInDb.Sum(f => f.FileSize);
//                // 2. รวมขนาดไฟล์ที่อัปโหลดเข้ามาใหม่
//                var uploadedSize = files.Sum(f => f.Length);
//                var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSizeInBytes", 5242880); // 5MB default
//                var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
//                    ?? new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".txt" };
//                var totalSize = total + uploadedSize;
//                if (uploadedSize > maxFileSize)
//                {
//                    response.Success = false;
//                    response.Message = "FileSize Must less than 5MB";
//                    return response;
//                }
//                if (totalSize > maxFileSize)
//                {
//                    float MBFree = (maxFileSize - total) / 1024f / 1024f; // Convert to MB
//                    response.Success = false;
//                    response.Message = $"PO Limit total filesize is 5MB. Free space now {MBFree.ToString("0.##")} MB.";
//                    return response;
//                }

//                foreach (var file in files)
//                {
//                    try
//                    {
//                        // Validate file size
//                        if (file.Length > maxFileSize)
//                        {
//                            _logger.LogWarning($"File {file.FileName} exceeds maximum size limit");
//                            response.Success = false;
//                            response.Message = "FileSize Must less than 5MB";
//                            return response;
//                        }

//                        // Validate file extension
//                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
//                        if (!allowedExtensions.Contains(fileExtension))
//                        {
//                            _logger.LogWarning($"File {file.FileName} has unsupported extension");
//                            continue;
//                        }

//                        // Generate unique filename
//                        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
//                        var filePath = Path.Combine(uploadPath, uniqueFileName);

//                        // Save file to disk
//                        using (var stream = new FileStream(filePath, FileMode.Create))
//                        {
//                            await file.CopyToAsync(stream);
//                        }

//                        // Create database record
//                        var relativePath = Path.Combine(folderType, poNo, uniqueFileName)
//    .Replace("\\", "/"); // ใช้ / เพื่อให้ดูเป็น URL-friendly และไม่ติดปัญหา cross-platform

//                        var fileAttachment = new PO_FileAttachment
//                        {
//                            Filename = uniqueFileName,
//                            OriginalName = file.FileName,
//                            Type = file.ContentType,
//                            UploadDate = DateTime.Now,
//                            Url = relativePath,
//                            PONo = poNo,
//                            FileSize = (int)file.Length,
//                            UploadByType = (byte)uploadType // Assuming uploadType is an enum or int representing the type of uploader
//                        };

//                        _context.PO_FileAttachments.Add(fileAttachment);
//                        await _context.SaveChangesAsync();

//                        // Add to response
//                        response.UploadedFiles.Add(new FileUploadDto
//                        {
//                            Id = fileAttachment.Id,
//                            Filename = fileAttachment.Filename,
//                            OriginalName = fileAttachment.OriginalName,
//                            Type = fileAttachment.Type,
//                            UploadDate = fileAttachment.UploadDate,
//                            Url = fileAttachment.Url,
//                            PONo = fileAttachment.PONo,
//                            FileSize = fileAttachment.FileSize
//                        });

//                        _logger.LogInformation($"File {file.FileName} uploaded successfully for PO {poNo}");
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, $"Failed to upload file {file.FileName} for PO {poNo}");
//                        response.Success = false;
//                        response.Message = $"{ex.Message} : Failed to upload file {file.FileName} for PO {poNo}";
//                        return response;
//                    }
//                }

//                var userList = _context.Users.Include(x => x.Role);
//                var SuppCode = await _context.PO_Mains.FirstOrDefaultAsync(t => t.PONo == poNo);
//                List<User> userListResult = new();
//                string typeName = "";
//                if (uploadType == 1)
//                {
//                    userListResult = await userList.Where(t => t.Role.RoleName != "User").ToListAsync();
//                    typeName = $"Supplier : {SuppCode.SuppCode}";
//                }
//                if (uploadType == 2)
//                {
//                    userListResult = await userList.Where(t => t.Role.RoleName == "User" && t.supplierId == SuppCode.SuppCode).ToListAsync();
//                    typeName = "Purchase Officer";
//                }

//                if (userListResult.Any())
//                {
//                    // 1. สร้าง Notification หลัก
//                    var newNoti = new PO_Notifications
//                    {
//                        noti_id = Guid.NewGuid(),
//                        title = "PO UploadFile",
//                        message = $"PO เลขที่ {poNo} มีการอัพโหลดไฟล์โดย {typeName}",
//                        type = "UploadFile",
//                        refId = poNo,
//                        refType = "PO",
//                        createAt = DateTime.UtcNow,
//                        createBy = "system" // หรือ user ปัจจุบัน
//                    };

//                    // 2. สร้าง Receiver สำหรับทุกคน
//                    foreach (var u in userListResult)
//                    {
//                        newNoti.Receivers.Add(new PO_NotificationReceiver
//                        {
//                            noti_recvId = Guid.NewGuid(),
//                            noti_id = newNoti.noti_id,
//                            userId = u.userId,
//                            isRead = false,
//                            isArchived = false,
//                            readAt = DateTime.UtcNow
//                        });
//                    }

//                    _context.PO_Notifications.Add(newNoti);
//                    await _context.SaveChangesAsync();
//                }

//                if (uploadType == 1)
//                {
//                    await _hub.Clients.Group("master").SendAsync("ReceiveMessage", "Info", $"PO {poNo} has been UploadFile by supplier {SuppCode.SuppCode}.");
                        
//                }
//                if (uploadType == 2)
//                {
//                    await _hub.Clients.Group($"supplier-{SuppCode.SuppCode}").SendAsync("ReceiveMessage", "Info", $"PO {poNo} has been UploadFile by Purchase Officer.");

//                }

//                response.Success = response.UploadedFiles.Any();
//                response.Message = response.Success
//                    ? $"Successfully uploaded {response.UploadedFiles.Count} file(s)"
//                    : "No files were uploaded";

//                return response;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error uploading files for PO {poNo}");
//                response.Success = false;
//                response.Message = "An error occurred while uploading files";
//                return response;
//            }
//        }


//        public async Task<List<FileUploadDto>> GetFilesByPONoAsync(string poNo, int uploadType)
//        {
//            try
//            {
//                var files = await _context.PO_FileAttachments
//                    .Where(f => f.PONo == poNo && f.UploadByType == uploadType)
//                    .OrderByDescending(f => f.UploadDate)
//                    .Select(f => new FileUploadDto
//                    {
//                        Id = f.Id,
//                        Filename = f.Filename,
//                        OriginalName = f.OriginalName,
//                        Type = f.Type,
//                        UploadDate = f.UploadDate,
//                        Url = f.Url,
//                        PONo = f.PONo,
//                        FileSize = f.FileSize
//                    })
//                    .ToListAsync();

//                return files;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error retrieving files for PO {poNo}");
//                return new List<FileUploadDto>();
//            }
//        }

//        public async Task<bool> DeleteFileAsync(int fileId)
//        {
//            try
//            {
//                var fileAttachment = await _context.PO_FileAttachments
//                    .FirstOrDefaultAsync(f => f.Id == fileId);

//                if (fileAttachment == null)
//                {
//                    return false;
//                }

//                // Delete physical file
//                var filePath = Path.Combine(_localBasePath, fileAttachment.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));
//                if (File.Exists(filePath))
//                {
//                    File.Delete(filePath);
//                }

//                // Delete database record
//                _context.PO_FileAttachments.Remove(fileAttachment);
//                await _context.SaveChangesAsync();

//                _logger.LogInformation($"File {fileAttachment.Filename} deleted successfully");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error deleting file");
//                return false;
//            }
//        }

//        public async Task<FileStream> DownloadFileAsync(int fileId)
//        {

//            var fileAttachment = await _context.PO_FileAttachments.FindAsync(fileId);
//            if (fileAttachment == null)
//            {
//                return null;
//            }

//            //var filePath = Path.Combine(_networkPath, fileAttachment.Url.TrimStart('/'));
//            var filePath = Path.Combine(_localBasePath, fileAttachment.Url.Replace("/", Path.DirectorySeparatorChar.ToString()));

//            if (!File.Exists(filePath))
//            {
//                return null;
//            }

//            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
//        }

//        public async Task<FileUploadDto> GetFileByIdAsync(int fileId)
//        {
//            try
//            {
//                var fileAttachment = await _context.PO_FileAttachments.FindAsync(fileId);
//                if (fileAttachment == null)
//                {
//                    return null;
//                }

//                return new FileUploadDto
//                {
//                    Id = fileAttachment.Id,
//                    Filename = fileAttachment.Filename,
//                    OriginalName = fileAttachment.OriginalName,
//                    Type = fileAttachment.Type,
//                    UploadDate = fileAttachment.UploadDate,
//                    Url = fileAttachment.Url,
//                    PONo = fileAttachment.PONo,
//                    FileSize = fileAttachment.FileSize,
//                    Remark = fileAttachment.Remark
//                };
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }

//        }

//        public async Task<bool> UpdateDescriptionAsync(int FileId,string description)
//        {
//            try{

//                var fileAttachment = await _context.PO_FileAttachments.FindAsync(FileId);
//                if (fileAttachment == null)
//                {
//                    return false;
//                }
//                fileAttachment.Remark = description;
//                _context.PO_FileAttachments.Update(fileAttachment);
//                await _context.SaveChangesAsync();
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error updating file description for file ID {FileId}");
//                return false;
//            }
//        }
//    }
//}
