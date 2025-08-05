using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PO_Api.Data;
using PO_Api.Data.DTO.Request;

namespace PO_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly EmailService _emailService;
        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
            _emailService = new EmailService(
                host: "mail.yehpattana.com",
                port: 587,
                username: "jarvis-ypt@Ta-yeh.com",
                password: "J!@#1028", // ต้องเป็น App Password
                fromEmail: "jarvis-ypt@ta-yeh.com",
                displayFromEmail: "YPT PO ระบบสมาชิก"
            );
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var expiry = DateTime.UtcNow.AddDays(7);
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            SetCookie("access_token", accessToken, minutes: 10);
            SetCookie("refresh_token", refreshToken, expire: expiry);
            SetCookie("auth_status", "authenticated", expire: expiry, httpOnly: false);
            await _jwt.SaveRefreshTokenAsync(user, refreshToken);

            return Ok(new
            {
                user,
                message = "Login successful",
                //access_token = accessToken,
                //accessToken,
            });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var accessToken = Request.Cookies["access_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            // ตรวจสอบว่ามี refresh token หรือไม่
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh token not provided.");

            // หา refresh token ในฐานข้อมูล (เช็คทั้ง revoked และ expired)
            var token = await _db.RefreshTokens.FirstOrDefaultAsync(r =>
                r.Token == refreshToken && !r.IsRevoked &&
                r.ExpiresAt > DateTime.Now);

            if (token == null)
            {
                DeleteCookie("access_token");
                DeleteCookie("refresh_token");
                SetCookie("auth_status", "", days: -1); // Clear cookie
                return Unauthorized("Invalid or expired refresh token.");
            }
                

            // ตรวจสอบ refresh token กับ user
            var userId = token.UserId;
            var isValid = await _jwt.ValidateRefreshTokenAsync(userId, refreshToken);
            if (!isValid)
                return Unauthorized("Invalid refresh token.");

            // ถ้าต้องการตรวจสอบ access token ด้วย
            if (!string.IsNullOrEmpty(accessToken))
            {
                var principal = _jwt.GetPrincipalFromExpiredToken(accessToken);
                if (principal != null)
                {
                    var tokenUserId = principal.FindFirst("userId")?.Value;
                    if (!string.IsNullOrEmpty(tokenUserId) && tokenUserId != userId.ToString())
                    {
                        return BadRequest("Token mismatch.");
                    }
                }
            }
            // หา user
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            // สร้าง token ใหม่
            var newAccessToken = _jwt.GenerateAccessToken(user);
            var newRefreshToken = _jwt.GenerateRefreshToken();

            try
            {
                // Revoke refresh token เก่า
                token.IsRevoked = true;

                // บันทึก refresh token ใหม่
                await _jwt.SaveRefreshTokenAsync(user, newRefreshToken);

                // บันทึกการเปลี่ยนแปลง
                await _db.SaveChangesAsync();

                // ตั้งค่า Cookie ใหม่
                SetCookie("access_token", newAccessToken, minutes: 30);
                SetCookie("refresh_token", newRefreshToken, days: 7); // กำหนดวันหมดอายุคงที่
                SetCookie("auth_status", "authenticated", days: 7, httpOnly: false);

                return Ok(new
                {
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken,
                    message = "Tokens refreshed successfully"
                });
            }
            catch (Exception ex)
            {
                // Log error
                // _logger.LogError(ex, "Error refreshing tokens for user {UserId}", userId);
                return StatusCode(500, "Internal server error occurred while refreshing tokens");
            }
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return BadRequest("No user context");

            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var token = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
                if (token != null)
                {
                    token.IsRevoked = true;
                    await _db.SaveChangesAsync();
                }
            }

            DeleteCookie("access_token");
            DeleteCookie("refresh_token");
            SetCookie("auth_status", "", days: -1); // Clear cookie
            return Ok("Logged out successfully.");
        }


        [HttpPost("logincors")]
        public async Task<IActionResult> Logincors([FromQuery] int empNo)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == empNo);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var expiry = DateTime.UtcNow.AddDays(7);
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            SetCookie("access_token", accessToken, minutes: 10);
            SetCookie("refresh_token", refreshToken, expire: expiry);
            SetCookie("auth_status", "authenticated", expire: expiry, httpOnly: false);
            await _jwt.SaveRefreshTokenAsync(user, refreshToken);

            return Ok(new
            {
                user,
                message = "Login successful",
                //access_token = accessToken,
                //accessToken,
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                DeleteCookie("access_token");
                DeleteCookie("refresh_token");
                SetCookie("auth_status", "", days: -1); // Clear cookie
                return Unauthorized("Not authenticated");
            }

            var username = User.Identity.Name!;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("User not found");
            return Ok(new
            {
                user
            });
        }


        private void SetCookie(string name, string value, int? minutes = null, int? days = null,
                              DateTime? expire = null, bool httpOnly = true)
        {
            var opt = new CookieOptions
            {
                HttpOnly = httpOnly, // 🔧 เพิ่ม parameter
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            if (minutes.HasValue)
                opt.Expires = DateTimeOffset.Now.AddMinutes(minutes.Value);
            else if (days.HasValue)
                opt.Expires = DateTimeOffset.Now.AddDays(days.Value);
            else if (expire.HasValue)
                opt.Expires = expire;

            Response.Cookies.Append(name, value, opt);
        }
        private void DeleteCookie(string name)
        {
            Response.Cookies.Append(name, "", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(-1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        }
    }
}
