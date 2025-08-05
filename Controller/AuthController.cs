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

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
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
                user.userId,
                user.firstname,
                user.lastname,
                user.email,
                user.RoleId,
                rolename.RoleName,
                user.supplierId,
                suppliername.SupplierName,
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
                return Unauthorized("Not authenticated");

            var username = User.Identity.Name!;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return NotFound("User not found");
            return Ok(new
            {
                user.userId,
                user.firstname,
                user.lastname,
                user.username,
                user.email,
                user.RoleId,
                user.supplierId,
                suppliername.SupplierName,
                rolename.RoleName,
            });
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            try
            {
                var email = await _db.UserEmails.FirstOrDefaultAsync(u => u.email == request.Email);
                if (email == null)
                {
                    return NotFound("Email not found.");
                }
                var user = await _db.Users.Include(s => s.Emails).FirstOrDefaultAsync(t => t.userId == email.userId);
                if (user == null)
                {
                    return NotFound("User not found with the provided email.");
                }
                // Generate a reset token (you can implement your own logic here)
                var resetToken = Guid.NewGuid().ToString("N"); // 
                // Save the reset token to the database or send it via email
                
                var expires = DateTime.Now.AddMinutes(10);

                var passwordResetToken = new PO_PasswordResetToken
                {
                    token = resetToken,
                    email = request.Email,
                    expires = expires,
                    create_at = DateTime.Now,
                    user_id = user.userId // Assuming user_id is not nullable
                };
                
                await _db.PO_PasswordResetTokens.AddAsync(passwordResetToken);
                await _db.SaveChangesAsync();
                // Here you can send the reset link via email to the user
                // For demonstration, we will just return the reset link
                // http://localhost:3000/auth/resetpassword?token={resetToken}
                // https://www.ymt-group.com/PO_Website/auth/resetpassword?token={resetToken}
                var currentTemplate = new EmailTemplateDTO
                {
                    subject = "PO system reset password",
                    body = "ยืนยันการ reset password คลิ๊กที่ปุ่ม!!",
                    link = $"https://www.ymt-group.com/PO_Website/auth/resetpassword?token={resetToken}",
                    btnName = "Reset Password"
                };

                await _emailService.SendEmailAsync(
                    to: request.Email,
                    subject: "PO system reset password",
                    body: currentTemplate
                );

                return Ok(new
                {
                    success = true,
                    message = "Password reset link has been sent to your email.",
                    token = resetToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred while processing the request");
            }
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token)
        {
            try
            {
                var passwordResetToken = await _db.PO_PasswordResetTokens.FirstOrDefaultAsync(t => t.token == token);
                if(passwordResetToken == null)
                {
                    return NotFound("Invalid or expired password reset token.");
                }
                if(passwordResetToken.expires < DateTime.Now)
                {
                    return BadRequest("Password reset token has expired.");
                }
                if (passwordResetToken.used)
                {
                    return BadRequest("Password reset token is used");
                }
                return Ok(new
                {
                    message = "Password reset token is valid.",
                    token = passwordResetToken?.token,
                    expires = passwordResetToken?.expires,
                    userId = passwordResetToken?.user_id,
                    email = passwordResetToken?.email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred while processing the request");

            }
        }

        [HttpPost("loginCross")]
        public async Task<IActionResult> LoginCrossWeb([FromQuery] int userId)
        {
            try
            {
                var user = _db.UserCenters.FirstOrDefault(t => t.UserId == userId);
                if(user == null)
                {
                    return NotFound("Not found user in system");
                }

                var expiry = DateTime.Now.AddDays(7);
                var accessToken = _jwt.GenerateAccessToken(user);
                var refreshToken = _jwt.GenerateRefreshToken();

                SetCookie("access_token", accessToken, minutes: 1);
                SetCookie("refresh_token", refreshToken, expire: expiry);
                SetCookie("auth_status", "authenticated", expire: expiry, httpOnly: false);
                var Token = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };

                _db.RefreshTokens.Add(Token);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    user.Username,
                    firstName = user.FullName,
                    roleName = "PurchaseOfficer",
                    supplierName = "YPT Center",
                    message = "Login successful",
                    //accessToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred while processing the request");
            }
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
