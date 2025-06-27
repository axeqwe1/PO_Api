using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PO_Api.Data;
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

        public UserController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
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
                email = t.email,
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
            var user = await _db.Users.Include(u => u.Role).Select(t => new
            {
                userId = t.userId,
                firstName = t.firstname,
                lastName = t.lastname,
                username = t.username,
                password = t.password,
                email = t.email,
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
                password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                email = request.Email,
                RoleId = role.RoleId, // ✅ ถูกต้อง
                supplierId = request.SupplierId
            };

            _db.Add(newUser);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully",
            });
        }

        [Authorize]
        [HttpPut("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDTO dto)
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (role != "Admin" || role != "SupperAdmin")
                return Unauthorized("You are not authorized to access this resource.");

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            user.password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok("Password updated successfully");
        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestDTO request)
        {
            // ตรวจสอบว่าผู้ใช้มีอยู่หรือไม่
            var roleAuth = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (roleAuth != "Admin" || roleAuth != "SupperAdmin")
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
            user.email = request.Email;
            user.RoleId = role.RoleId;
            user.supplierId = request.SupplierId;
            // ถ้ามีการเปลี่ยนรหัสผ่าน ให้แฮชใหม่
            //if (!string.IsNullOrEmpty(request.Password))
            //{
            //    user.password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            //}
            _db.Users.Update(user);
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
            if (roleAuth != "Admin" || roleAuth != "SupperAdmin")
                return Unauthorized("You are not authorized to access this resource.");
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            _db.Remove(user);
            await _db.SaveChangesAsync();

            return Ok("Delete successfully");
        }
    }
}
