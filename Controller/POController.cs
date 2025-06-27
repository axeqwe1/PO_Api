using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PO_Api.Data;
using PO_Api.Models;

namespace PO_Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class POController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public POController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }


        [HttpGet("GetAllPOPagin")]
        public async Task<IActionResult> GetAllPOPagin(string tab = "all", int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _db.PO_Mains.Include(p => p.Details)
                    .Include(p => p.Suppliers)
                    .Where(t => t.ClosePO == true);

                switch (tab.ToLower())
                {
                    case "pending":
                        query = query.Where(t => _db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo) == null || (_db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo) != null && _db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo).SuppRcvPO == false));
                        break;
                    case "confirm":
                        query = query.Where(t => t.ReceiveInfo != null && t.ReceiveInfo.SuppRcvPO == true);
                        break;
                    case "cancel":
                        query = query.Where(t => t.ReceiveInfo != null && t.ReceiveInfo.SuppCancelPO == 2);
                        break;
                    case "all":
                    default:
                        // no extra filter
                        break;
                }

                // Load PO_Mains พร้อม Include ทั้งหมด (Single EF query)
                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderByDescending(po => po.ApproveDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                // Mapping ให้อยู่ใน shape ที่ต้องการ
                var result = data.Select(po => new
                {
                    po.PONo,
                    po.CompanyCode,
                    po.SuppCode,
                    po.SuppContact,
                    po.ClosePO,
                    po.ApproveDate,
                    po.CancelPO,
                    po.POReady,
                    po.FinalETADate,
                    SupplierName = po.Suppliers?.SupplierName,
                    Details = po.Details.Select(t => new
                    {
                        t.MatrClass,
                        t.MatrCode,
                        t.Color,
                        t.Size,
                        t.Description,
                        t.FinalETADate,
                        t.TempId,
                    }).ToList(),
                    ReceiveInfo = po.ReceiveInfo == null ? null : new
                    {
                        po.ReceiveInfo.Remark,
                        po.ReceiveInfo.SuppRcvPO,
                        po.ReceiveInfo.SuppRcvDate,
                        po.ReceiveInfo.SuppCancelPO,
                        po.ReceiveInfo.ConfirmCancelBy,
                        po.ReceiveInfo.ConfirmCancelDate
                    }
                }).ToList();


                var countAll = await _db.PO_Mains.CountAsync(t => t.ClosePO == true);
                var countPending = await _db.PO_Mains.CountAsync(t => t.ClosePO == true && (_db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo) == null || (_db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo) != null && _db.PO_SuppRcvs.FirstOrDefault(r => r.PONo == t.PONo).SuppRcvPO == false)));
                var countConfirm = await _db.PO_Mains.CountAsync(t => t.ClosePO == true && t.ReceiveInfo != null && t.ReceiveInfo.SuppRcvPO == true);
                var countCancel = await _db.PO_Mains.CountAsync(t => t.ClosePO == true && t.ReceiveInfo != null && t.ReceiveInfo.SuppCancelPO == 1);
                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    data = result,
                    counts = new
                    {
                        all = countAll,
                        pending = countPending,
                        confirm = countConfirm,
                        cancel = countCancel,
                    }

                    //page,
                    //pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("GetAllPO")]
        public async Task<IActionResult> GetAllPO()
        {
            try
            {
                var items = await _db.PO_Mains
                    .Where(po => po.POReady == true)
                    .OrderByDescending(po => po.ApproveDate)
                    .Select(po => new
                    {
                        po.PONo,
                        po.CompanyCode,
                        po.SuppCode,
                        po.SuppContact,
                        po.ClosePO,
                        po.ApproveDate,
                        po.CancelPO,
                        po.POReady,
                        po.FinalETADate,
                        SupplierName = po.Suppliers.SupplierName,
                        ReceiveInfo = _db.PO_SuppRcvs.FirstOrDefault(t => t.PONo == po.PONo),
                        Files = _db.PO_FileAttachments
                            .Where(f => f.PONo == po.PONo) // 1 = PO File
                            .Select(f => new
                            {
                                f.Id,
                                f.Filename,
                                f.Type,
                                f.UploadDate,
                                f.Url,
                                f.OriginalName,
                                f.FileSize,
                                f.UploadByType
                            }).ToList(),
                    })
                    .ToListAsync();

                return Ok(new { items });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetPO/{suppCode}")]
        public async Task<IActionResult> GetPO(string suppCode)
        {
            try
            {

                var query = _db.PO_Mains
                    .Include(p => p.Details)
                    .Include(p => p.ReceiveInfo)
                    .Where(t => t.POReady == true && t.SuppCode == suppCode)
                    .OrderByDescending(po => po.ApproveDate)
                    .Select(po => new
                    {
                        po.PONo,
                        po.CompanyCode,
                        po.SuppCode,
                        po.SuppContact,
                        po.ClosePO,
                        po.ApproveDate,
                        po.CancelPO,
                        po.POReady,
                        po.FinalETADate,
                        SupplierName = po.Suppliers.SupplierName,
                        ReceiveInfo = _db.PO_SuppRcvs.FirstOrDefault(t => t.PONo == po.PONo),
                        Details = _db.PO_Details
                            .Where(d => d.PONo == po.PONo)
                            .ToList(), // ยังใช้ ToList ได้แต่ควร paginate ภายหลัง

                    });

                // Pagination

                var items = await query
                    .ToListAsync();


                return Ok(new
                {
                    items,
                    //page,
                    //pageSize
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetPODetails/{poNo}")]
        public async Task<IActionResult> GetPODetails(string poNo)
        {
            try
            {
                var details = await _db.PO_Details
                    .Where(d => d.PONo == poNo)
                    .Select(d => new
                    {
                        d.PONo,
                        d.MatrClass,
                        d.MatrCode,
                        d.Color,
                        d.Size,
                        d.Description,
                        d.FinalETADate,
                        d.TempId
                    })
                    .ToListAsync();


                return Ok(new
                {
                    Details = details,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetPOCancelDatas")]
        public async Task<IActionResult> GetPOCancelList()
        {
            try
            {
                var query = await _db.PO_Mains.Include(p => p.ReceiveInfo).Include(m => m.Suppliers).Where(s => s.ReceiveInfo.SuppCancelPO == 1).Select(t => new
                {
                    t.PONo,
                    t.Suppliers.SupplierName,
                    t.SuppCode,
                    t.ReceiveInfo.Remark,
                    t.ReceiveInfo.SuppCancelDate,
                    t.ReceiveInfo.ConfirmCancelBy,
                    t.ReceiveInfo.ConfirmCancelDate,
                }).ToListAsync();

                return Ok(query);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("SaveDownloadStatus/{PONo}")]
        public async Task<IActionResult> UpdateStatus(string PONo)
        {
            try
            {
                var data = await _db.PO_Mains.FirstOrDefaultAsync(t => t.PONo == PONo);
                var poStatus = await _db.PO_SuppRcvs.FirstOrDefaultAsync(t => t.PONo == PONo);

                if (data == null)
                {
                    return BadRequest("Not Found PO");
                }

                if (poStatus != null)
                {
                    return BadRequest("This File Already Download");
                }

                var newStatus = new PO_SuppRcv
                {
                    PONo = PONo,
                    SuppRcvDate = DateTime.Now,
                    SuppRcvPO = true,
                };

                await _db.PO_SuppRcvs.AddAsync(newStatus);
                await _db.SaveChangesAsync();

                return Ok("Update Success");
            }
            catch (Exception ex)
            {
                // ส่งกลับข้อความ error (หรือ Log)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("RequestCancel")]
        public async Task<IActionResult> RequestCancel(string PONo, string Remark)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PONo))
                {
                    return BadRequest("Not Found PO Number in Request");
                }
                if (string.IsNullOrWhiteSpace(Remark))
                {
                    return BadRequest("Please Enter Reason for Cancel");
                }
                var query = await _db.PO_Mains.FirstOrDefaultAsync(t => t.PONo == PONo);
                var poStatus = await _db.PO_SuppRcvs.FirstOrDefaultAsync(t => t.PONo == PONo);
                if (query == null)
                {
                    return NotFound("Not Found Data PO in Database");
                }
                if (poStatus == null)
                {
                    _db.PO_SuppRcvs.Add(new PO_SuppRcv
                    {
                        PONo = PONo,
                        SuppRcvPO = false,
                        SuppCancelPO = 1,
                        SuppCancelDate = DateTime.Now,
                        Remark = Remark
                    });
                }
                else
                {
                    if (poStatus.SuppCancelPO == 1)
                    {
                        return BadRequest("This PO Already Request Cancel");
                    }
                    if (poStatus.SuppCancelPO == 2)
                    {
                        return BadRequest("This PO Already Cancel");
                    }
                    poStatus.SuppCancelDate = DateTime.Now;
                    poStatus.Remark = Remark;
                    poStatus.SuppCancelPO = 1;
                    _db.PO_SuppRcvs.Update(poStatus);
                }

                await _db.SaveChangesAsync();
                return Ok("Send Request Cancel Success");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
