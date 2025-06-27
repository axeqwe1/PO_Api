using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PO_Api.Data;
using PO_Api.Data.DTO.Request;

namespace PO_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public RoleController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            if (roles == null || !roles.Any())
                return NotFound("No roles found");
            return Ok(roles);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("CreateRole")]

        public async Task<IActionResult> CreateRole(CreateRoleRequestDTO request)
        {
            if (request.RoleName == null || request.RoleName.Trim() == "")
                return BadRequest("Role name cannot be empty");

            _db.Roles.Add(new Models.Role
            {
                RoleName = request.RoleName.Trim()
            });

            await _db.SaveChangesAsync();
            return Ok("Create Role Success");

        }
    }
}
