//using System.Reflection.Metadata;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using PO_Api.Data;
//using PO_Api.Data.DTO.Request;
//using PO_Api.Hubs;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace PO_Api.Controller
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class NotificationController : ControllerBase
//    {
//        private readonly IHubContext<NotificationHub> _hub;
//        private readonly AppDbContext _db;
//        private readonly JwtService _jwt;
//        public NotificationController(AppDbContext db, JwtService jwt, IHubContext<NotificationHub> hub)
//        {
//            _db = db;
//            _jwt = jwt;
//            _hub = hub;
//        }

//        [HttpGet("GetNotify/{userId}")]
//        public async Task<IActionResult> GetNotify(int userId)
//        {
//            try
//            {
//                var user = await _db.Users.FindAsync(userId);
//                if (user == null)
//                {
//                    return NotFound(new { success = false, message = "User not found" });
//                }
//                var notifyCount = await _db.PO_NotificationReceivers
//                    .Include(x => x.Notification)
//                    .Where(c => c.userId == userId && !c.isRead && !c.isArchived)
//                    .CountAsync();
//                return Ok(new { success = true, count = notifyCount });
//            }
//            catch (Exception ex) {

//                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
//            }
//        }

//        [HttpGet("GetNotifyData/{userId}")]
//        public async Task<IActionResult> GetNotifyData(int userId)
//        {
//            try
//            {
//                var user = await _db.Users.FindAsync(userId);
//                if (user == null)
//                {
//                    return NotFound(new { success = false, message = "User not found" });
//                }
//                var notify = await _db.PO_NotificationReceivers
//                    .Include(x => x.Notification)
//                    .Where(c => c.userId == userId && !c.isArchived)
//                    .Select(t => new
//                    {
//                        noti_id = t.noti_id,
//                        noti_recvId = t.noti_recvId,
//                        isRead = t.isRead,
//                        readAt = t.readAt,
//                        isArchived = t.isArchived,
//                        Notification = new
//                        {
//                            title = t.Notification.title,
//                            message = t.Notification.message,
//                            type = t.Notification.type,
//                            refId = t.Notification.refId,
//                            refType = t.Notification.refType,
//                            createAt = t.Notification.createAt,
//                            createBy = t.Notification.createBy,
//                        },
                    
//                    }).ToListAsync();
//                var countIsArchived = await _db.PO_NotificationReceivers.Where(q => q.userId == userId && q.isArchived).CountAsync();
//                var countIsRead = await _db.PO_NotificationReceivers.Where(q => q.userId == userId && q.isRead).CountAsync();
//                var countIsNotRead = await _db.PO_NotificationReceivers.Where(q => q.userId == userId && !q.isRead).CountAsync();
//                return Ok(new { success = true, data = new { notify , countIsArchived , countIsRead, countIsNotRead } });
//            }
//            catch (Exception ex)
//            {

//                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
//            }
//        }

//        [HttpPost("notify-po-updated")]
//        public async Task<IActionResult> NotifyPoUpdated(NotifyRequest req)
//        {
//            var data = _db.PO_Mains.FirstOrDefault(x => x.PONo == req.PONo);
//            if (data == null)
//            {
//                await _hub.Clients.All.SendAsync("ReceiveMessage", req.PONo, "Not found");
//                return NotFound("PO not found");
//            }

//            string groupName = $"supplier-{data.SuppCode}";
//            string message = $"PO {req.PONo} has been updated.";

//            await _hub.Clients.Group(groupName).SendAsync("ReceiveMessage", "PO System", message);

//            return Ok("Notification sent");
//        }

//        [HttpPost("notify")]
//        public async Task<IActionResult> NotifyAll()
//        {
//            await _hub.Clients.All.SendAsync("ReceiveMessage", "Admin", "Hello from server!");
//            return Ok("Sent!");
//        }
//        // ส่งถึงทุกคน
//        [HttpPost("broadcast")]
//        public async Task<IActionResult> Broadcast([FromBody] string message)
//        {
//            await _hub.Clients.All.SendAsync("ReceiveMessage", "System", message);
//            return Ok("Broadcast sent");
//        }

//        // ส่งถึง user เดียว
//        [HttpPost("to-user/{userId}")]
//        public async Task<IActionResult> SendToUser(string userId, [FromBody] string message)
//        {
//            await _hub.Clients.User(userId).SendAsync("ReceiveMessage", "System", message);
//            return Ok($"Message sent to user: {userId}");
//        }

//        [HttpPost("MarkAsRead")]
//        public async Task<IActionResult> MarkAsRead([FromBody] Guid[] req)
//        {
//            try
//            {

//                foreach (var item in req)
//                {
//                    var query = await _db.PO_NotificationReceivers.Where(t => t.noti_recvId == item).FirstOrDefaultAsync();
//                    if(query == null)
//                    {
//                        return NotFound("Not found Data");
//                    }
//                    query.isRead = true;
//                    query.readAt = DateTime.Now;
//                    _db.Update(query);
                    
//                }
//                await _db.SaveChangesAsync();
//                return Ok(new { success = true, message = "Mark AS Read Success" });
//            }
//            catch (Exception ex) { 
            
//                return BadRequest(ex.Message);
//            }
//        }

//        [HttpPost("Archived")]
//        public async Task<IActionResult> Archived([FromBody] Guid[] req)
//        {
//            try
//            {
//                foreach (var item in req)
//                {
//                    var query = await _db.PO_NotificationReceivers.Where(t => t.noti_recvId == item).FirstOrDefaultAsync();
//                    if (query == null)
//                    {
//                        return NotFound("Not found Data");
//                    }
//                    if (!query.isRead)
//                    {
//                        query.isRead = true;
//                        query.readAt = DateTime.Now;
//                    }
//                    query.isArchived = true;
                    
//                    _db.Update(query);

//                }
//                await _db.SaveChangesAsync();
//                return Ok(new { success = true, message = "Archived Success" });
//            }
//            catch (Exception ex)
//            {

//                return BadRequest(ex.Message);
//            }
//        }

//        [HttpPost("UpdatePending/{PONo}")]
//        public async Task<IActionResult> UpdatePending(string PONo)
//        {
//            try
//            {
//                var query = await _db.PO_Mains.FirstOrDefaultAsync(t => t.PONo == PONo);
//                if(query == null)
//                {
//                    return NotFound("Not Found PO");
//                }
//                var userList = await _db.Users.Include(s => s.Role).Where(t => t.supplierId == query.SuppCode && t.Role.RoleName == "User").ToListAsync();
//                if (userList.Any())
//                {
//                    // 1. สร้าง Notification หลัก
//                    var newNoti = new PO_Notifications
//                    {
//                        noti_id = Guid.NewGuid(),
//                        title = "PO Update Status",
//                        message = $"PO เลขที่ {PONo} Update สถานะเป็น Pending แล้ว",
//                        type = "Update",
//                        refId = PONo,
//                        refType = "PO",
//                        createAt = DateTime.UtcNow,
//                        createBy = "system" // หรือ user ปัจจุบัน
//                    };

//                    // 2. สร้าง Receiver สำหรับทุกคน
//                    foreach (var u in userList)
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

//                    _db.PO_Notifications.Add(newNoti);
//                    await _db.SaveChangesAsync();
//                }
//                string groupName = $"supplier-{query.SuppCode}";
//                string message = $"PO {PONo} has been updated status to pending.";

//                await _hub.Clients.Group(groupName).SendAsync("ReceiveMessage", "Info", message);
//                return Ok(new { success = true, message =  $"Send pending success {PONo}" });
//            }
//            catch (Exception ex) {
//                return BadRequest(ex.Message);
//            }
//        }

//        public class MarkAsReadRequest
//        {
//            public Guid notiRecvId { get; set; }
//        }

//    }
//}
