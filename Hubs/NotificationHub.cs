//using System.Security.Claims;
//using Microsoft.AspNetCore.SignalR;
//using PO_Api.Data;

//namespace PO_Api.Hubs
//{
//    public class NotificationHub : Hub
//    {
//        private readonly AppDbContext _db;

//        // Join group (ตอน login หรือ onConnected)
//        public NotificationHub(AppDbContext db)
//        {
//            _db = db;
//        }
//        public override async Task OnConnectedAsync()
//        {
//            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

//            if (!string.IsNullOrEmpty(userId))
//            {
//                var user = await _db.Users.FindAsync(int.Parse(userId));
//                if(user == null)
//                {
//                    Console.WriteLine($"❌ User with ID {userId} not found.");
//                    return;
//                }
//                else
//                {
//                    if (role != null && role != "User")
//                    {
//                        var GroupName = $"master";
//                        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
//                        Console.WriteLine($"✅ {user.username} joined group {GroupName}");
//                    }
//                    else
//                    {
//                        if (user != null && !string.IsNullOrEmpty(user.supplierId))
//                        {
//                            var groupName = $"supplier-{user.supplierId}";
//                            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
//                            Console.WriteLine($"✅ {user.username} joined group {groupName}");

//                        }
//                    }
//                }  

//            }

//            await base.OnConnectedAsync();
//        }
//        public async Task SendMessage(string user, string message)
//        {
//            await Clients.All.SendAsync("ReceiveMessage", user, message);
//        }

//        // กรณีอยากส่งหาเฉพาะ User
//        public async Task SendToUser(string userId, string message)
//        {
//            await Clients.User(userId).SendAsync("ReceiveMessage", message);
//        }
//    }
//}
