using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PO_Api.Data;
using PO_Api.Data.DTO;
using PO_Api.Data.DTO.Request;
using PO_Api.Models;

namespace PO_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly EmailService _emailService;
        public UserController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
            _emailService = new EmailService(
                host: "mail.yehpattana.com",
                port: 587,
                username: "jarvis-ypt@Ta-yeh.com",
                password: "J!@#1028", // ต้องเป็น App Password
                fromEmail: "jarvis-ypt@ta-yeh.com",
                displayFromEmail: "YPTPO ระบบสมาชิก"
            );
        }

        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            // ดึงข้อมูลผู้ใช้ทั้งหมด
            var users = await _db.Users.Include(u => u.Role).Select(t => new
            {
                userId = t.userId,
                firstName = t.firstname,
                lastName = t.lastname,
                username = t.username,
                email = t.Emails.Select(w => w.email).ToList(),
                role = t.Role.RoleName,
                roleId = t.Role.RoleId,
                supplierCode = t.supplierId
            }).ToListAsync();
            return Ok(new
            {
                data = users,
                message = "Users retrieved successfully",
            });
        }

        [Authorize]
        [HttpGet("GetId/{id}")]
        public async Task<IActionResult> GetId(int id)
        {
            // ดึงข้อมูลผู้ใช้ทั้งหมด
            var user = await _db.Users.Include(u => u.Role).Include(a => a.Emails).Select(t => new
            {
                userId = t.userId,
                firstName = t.firstname,
                lastName = t.lastname,
                username = t.username,
                password = t.password,
                email = t.Emails.Select(w => w.email).ToList(),
                role = t.Role.RoleName,
                roleId = t.Role.RoleId,
                supplierCode = t.supplierId
            }).FirstOrDefaultAsync(t => t.userId == id);
            return Ok(new
            {
                data = user,
                message = "Users retrieved successfully",
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            // ตรวจสอบว่าผู้ใช้มีอยู่แล้วหรือไม่
            var existingUser = await _db.Users
                .FirstOrDefaultAsync(u => u.username == request.Username);
            if (existingUser != null)
                return BadRequest("User already exists");

            // ตรวจสอบว่ามี Role ตรงกับที่ระบุไหม
            var role = await _db.Roles.FirstOrDefaultAsync(w => w.RoleName == request.RoleName);
            if (role == null)
                return BadRequest($"Role '{request.RoleName}' does not exist");


            var newUser = new User
            {
                firstname = request.FirstName,
                lastname = request.LastName,
                username = request.Username,
                password = BCrypt.Net.BCrypt.HashPassword("YPT001"),
                RoleId = role.RoleId,
                supplierId = request.SupplierId
            };

            // บันทึก user ก่อน เพื่อให้ได้ userId จาก DB
            await _db.AddAsync(newUser);
            await _db.SaveChangesAsync(); // ตอนนี้ newUser.userId จะมีค่าแล้ว

            foreach (var email in request.Email)
            {
                var NewEmail = new UserEmail
                {
                    userId = newUser.userId, // ✅ ตอนนี้มีค่าแล้ว
                    emailId = Guid.NewGuid(),
                    email = email,
                    isActive = true,
                };
                await _db.AddAsync(NewEmail);
            }

            await _db.SaveChangesAsync(); // save email ทีหลัง

            var expires = DateTime.Now.AddMinutes(10);
            var tokens = new List<PO_PasswordResetToken>();
            var dateTime = DateTime.Now.ToString("dd MMMM yyyy 'เวลา' HH:mm", new CultureInfo("th-TH"));
            foreach (var email in request.Email)
            {
                var resetToken = Guid.NewGuid().ToString("N"); // 

                var passwordResetToken = new PO_PasswordResetToken
                {
                    token = resetToken,
                    email = email,
                    expires = expires,
                    create_at = DateTime.Now,
                    user_id = newUser.userId // Assuming user_id is not nullable
                };
                tokens.Add(passwordResetToken);

                var currentTemplate = new EmailTemplateDTO
                {
                    subject = "PO system register",
                    body = $"<div>\r\n  <div style=\"display: block; margin-bottom: 8px\">\r\n    ระบบได้เพิ่ม Email ของคุณเข้าสู่ระบบ PO\r\n  </div>\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    บัญชีของคุณ (username: {newUser.username}) ได้ลงทะเบียนใช้งานระบบ PO เมื่อวันที่\r\n    {dateTime} ดำเนินการโดย: ระบบ\r\n  </div>\r\n  <br />\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    หากคุณเป็นผู้ดำเนินการเอง คุณสามารถละข้อความนี้ได้\r\n  </div>\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    Password ตั้งต้นให้กับคุณคือ YPT001 กรุณาเปลี่ยน password\r\n    เพื่อความปลอดภัยของข้อมูล\r\n  </div>\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    👉\r\n    <a href=\"https://www.ymt-group.com/PO_Website\" target=\"_blank\"\r\n      >[เข้าสู่ระบบระบบ PO]</a\r\n    >\r\n  </div>\r\n</div>\r\n",
                    link = $"https://www.ymt-group.com/PO_Website/auth/resetpassword?token={resetToken}",
                    btnName = "Reset Password!"
                };

                await _emailService.SendEmailAsync(
                    to: email,
                    subject: "PO system ลงทะเบียนเข้าใช้งานระบบ PO",
                    body: currentTemplate
                );
            }
            await _db.PO_PasswordResetTokens.AddRangeAsync(tokens);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "User registered successfully",
            });
        }

        [HttpPut("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDTO dto)
        {

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");
            var token = await _db.PO_PasswordResetTokens.FirstOrDefaultAsync(t => t.token == dto.TokenResetPassword);
            if(token != null)
            {
                token.used = true;
                await _db.SaveChangesAsync();
            }
            string userChange = "";
            if (!string.IsNullOrEmpty(dto.Email)) {
                userChange = dto.Email;
            }
            else
            {
                userChange = "ระบบ";
            }
            user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var dateTime = DateTime.Now.ToString("dd MMMM yyyy 'เวลา' HH:mm", new CultureInfo("th-TH"));
            var currentTemplate = new EmailTemplateDTO
            {
                subject = "PO system แจ้งการเปลี่ยน Password",
                body = $@"
                        <div>
                            <div style=""display: block; margin-bottom: 8px"">
                            มีการเปลี่ยนรหัสผ่านของบัญชีของคุณ
                            </div>
                            <div style=""display: block; margin-bottom: 4px"">
                            บัญชีของคุณ (username: {user.username}) ได้มีการเปลี่ยนรหัสผ่านเมื่อวันที่
                            {dateTime} ดำเนินการโดย: {userChange}
                            </div>
                            <br />
                            <div style=""display: block; margin-bottom: 4px"">
                            หากคุณเป็นผู้ดำเนินการเอง คุณสามารถละข้อความนี้ได้
                            </div>
                            <div style=""display: block; margin-bottom: 4px"">
                            หากคุณและผู้เกี่ยวข้องไม่ใช่ผู้ดำเนินการ กรุณาเข้าสู่ระบบเพื่อตรวจสอบ และ เปลี่ยนรหัสผ่านทันที
                            </div>
                            <div style=""display: block; margin-bottom: 4px"">
                            👉
                            <a href=""https://www.ymt-group.com/PO_Website"" target=""_blank"">
                                [เข้าสู่ระบบระบบ PO]
                            </a>
                            </div>
                        </div>
                    "
            };
            var emails = await _db.UserEmails.Where(t => t.userId == user.userId).ToListAsync();

            foreach (var email in emails)
            {
                await _emailService.SendEmailAsync(
                    to: email.email,
                    subject: "PO system แจ้งการเปลี่ยน Password",
                    body: currentTemplate
                );
            }

            await _db.SaveChangesAsync();

            return Ok("Password updated successfully");
        }

        [Authorize]
        [HttpPut("{userId}/change-password-user")]
        public async Task<IActionResult> ChangePasswordUser(int userId, [FromBody] ChangePasswordDTO dto)
        {

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if(user.password != null && !BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.password))
                return BadRequest("Old password is incorrect");

            user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var currentTemplate = new EmailTemplateDTO
            {
                subject = "PO system แจ้งการเปลี่ยน Password",
                body = $"<div>\r\n  <div style=\"display: block; margin-bottom: 8px\">\r\n    รหัสเข้าใช้งานปัจจุบันของคุณ\r\n  </div>\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    username: {user.username}\r\n  </div>\r\n  <div style=\"display: block\">password: {dto.NewPassword}</div>\r\n</div>\r\n",
            };
            var emails = await _db.UserEmails.Where(t => t.userId == user.userId).ToListAsync();

            foreach (var email in emails)
            {
                await _emailService.SendEmailAsync(
                    to: email.email,
                    subject: "PO systeam แจ้งการเปลี่ยน Password",
                    body: currentTemplate
                );
            }
            await _db.SaveChangesAsync();

            return Ok("Password updated successfully");
        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestDTO request)
        {
            // ตรวจสอบว่าผู้ใช้มีอยู่หรือไม่
            var roleAuth = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (roleAuth != "Admin" && roleAuth != "SupperAdmin")
                return Unauthorized("You are not authorized to access this resource.");

            var user = await _db.Users.FindAsync(id);
            var role = await _db.Roles.FirstOrDefaultAsync(t => t.RoleName == request.RoleName);
            if (!string.IsNullOrEmpty(request.SupplierId))
            {
                var supplier = await _db.Suppliers.FirstOrDefaultAsync(t => t.SupplierCode == request.SupplierId);
                if (supplier == null)
                {
                    return BadRequest("Not Found Supplier Data");
                }
            }
            if (user == null)
                return NotFound("User not found");
            // อัปเดตข้อมูลผู้ใช้
            user.firstname = request.FirstName;
            user.lastname = request.LastName;
            user.RoleId = role.RoleId;
            user.supplierId = request.SupplierId;
            user.username = request.Username;

            var emailQuery = await _db.UserEmails.Where(t => t.userId == user.userId).ToListAsync();

            List<UserEmail> deleteRow = new List<UserEmail>(); 
            foreach(var email in emailQuery)
            {
                bool found = false;
                foreach (var item in request.Email)
                {
                    if(email.email == item)
                    {
                        found = true; break;
                    }
                }
                if (!found)
                {
                    deleteRow.Add(email);
                }
            }
            _db.UserEmails.RemoveRange(deleteRow);
            await _db.SaveChangesAsync();

            List<UserEmail> addRow = new List<UserEmail>();
            foreach (var email in request.Email)
            {
                bool found = false;
                foreach (var item in emailQuery)
                {
                    if (email == item.email)
                    {
                        found = true; break;
                    }

                }
                if (!found)
                {
                    var newEmail = new UserEmail { 
                        emailId = Guid.NewGuid(),
                        userId = user.userId,
                        email = email,
                        isActive = true,
                    };

                    addRow.Add(newEmail);
                }
            }


            // ถ้ามีการเปลี่ยนรหัสผ่าน ให้แฮชใหม่
            //if (!string.IsNullOrEmpty(request.Password))
            //{
            //    user.password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            //}
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            // Save the reset token to the database or send it via email

            var expires = DateTime.Now.AddMinutes(10);
            var tokens = new List<PO_PasswordResetToken>();
            foreach (var email in addRow)
            {
                var resetToken = Guid.NewGuid().ToString("N"); // 

                var passwordResetToken = new PO_PasswordResetToken
                {
                    token = resetToken,
                    email = email.email,
                    expires = expires,
                    create_at = DateTime.Now,
                    user_id = user.userId // Assuming user_id is not nullable
                };
                tokens.Add(passwordResetToken);
                var currentTemplate = new EmailTemplateDTO
                {
                    subject = "PO system email register",
                    body = $"<div>\r\n  <div style=\"display: block; margin-bottom: 8px\">\r\n    Email ของคุณได้ลงทะเบียนในระบบ PO แล้ว กรูณา Reset Password หรือ ขอ Password\r\n    จากบุคคลที่เกี่ยวข้อง\r\n  </div>\r\n  <div style=\"display: block; margin-bottom: 4px\">\r\n    <span\r\n      ><a href=\"https://www.ymt-group.com/PO_Website/\" target=\"_blank\"\r\n        >เข้าเว็บไซต์</a\r\n      ></span\r\n    >\r\n  </div>\r\n</div>\r\n",
                    link = $"https://www.ymt-group.com/PO_Website/auth/resetpassword?token={resetToken}",
                    btnName = "Reset Password!"
                };

                await _emailService.SendEmailAsync(
                    to: email.email,
                    subject: "PO system ลงทะเบียนเข้าใช้งานระบบ PO",
                    body: currentTemplate
                );
            }
            await _db.PO_PasswordResetTokens.AddRangeAsync(tokens);
            await _db.UserEmails.AddRangeAsync(addRow);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully",

            });
        }

        [Authorize]
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var roleAuth = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (roleAuth != "Admin" && roleAuth != "SupperAdmin")
                return Unauthorized("You are not authorized to access this resource.");
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var emails = await _db.UserEmails.Where(t => t.userId == user.userId).ToListAsync();
            foreach(var email in emails)
            {
                _db.UserEmails.Remove(email);
            }

            _db.Remove(user);
            await _db.SaveChangesAsync();

            return Ok("Delete successfully");
        }

        [Authorize]
        [HttpGet("GetEmail/{userId}")]
        public async Task<IActionResult> GetEmail(int userId)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null) return NotFound("Not found user data");

                var email = await _db.UserEmails.Where(t => t.userId == userId).Select(t => new
                {
                    t.emailId,
                    t.email,
                    t.isActive,
                }).ToListAsync();
                if (email == null) return NotFound("Not found email data");
                return Ok(new { success = true, data = email });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("setActiveEmail/{emailId}")]
        public async Task<IActionResult> SetActiveEmail(Guid emailId, [FromQuery] bool isActive)
        {
            try
            {
                var email = await _db.UserEmails.FindAsync(emailId);
                if (email == null)
                    return NotFound(new { message = "Email not found" });

                email.isActive = isActive;
                await _db.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
