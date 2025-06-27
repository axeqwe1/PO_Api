using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PO_Api.Data;

namespace PO_Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]

    public class SupplierController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public SupplierController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            //var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            //if (role != "Admin")
            //    return Unauthorized("You are not authorized to access this resource.");
            // ตรวจสอบว่าผู้ใช้มีอยู่แล้วหรือไม่
            var data = _db.Suppliers.ToListAsync();

            return Ok(new
            {
                data = data.Result,

            });
        }

    }
}
