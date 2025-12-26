using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Dtos; // Namespace chứa DTO

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonanController : ControllerBase
    {
        private readonly DataContext _context;

        public MonanController(DataContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH MÓN ĂN (Public)
        // GET: api/Menu
        // Có thể lọc theo danh mục: /api/Menu?danhMucId=1
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MonAnDto>>> GetMenu([FromQuery] int? danhMucId)
        {
            var query = _context.MonAn.Include(m => m.DanhMuc).AsQueryable();

            // Nếu có truyền ID danh mục thì lọc
            if (danhMucId.HasValue)
            {
                query = query.Where(m => m.danh_muc_id == danhMucId);
            }

            // Map sang DTO để trả về
            return await query.Select(m => new MonAnDto
            {
                Id = m.mon_id,
                TenMon = m.ten_mon,
                Gia = m.gia,
                AnhUrl = m.anh_url,
                MoTa = m.mo_ta,
                CoSan = m.co_san,
                DanhMucId = m.danh_muc_id,
                TenDanhMuc = m.DanhMuc.ten_danh_muc // Lấy tên danh mục từ bảng liên kết
            }).ToListAsync();
        }

        // 2. LẤY CHI TIẾT 1 MÓN (Public)
        // GET: api/Menu/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MonAnDto>> GetMonAn(int id)
        {
            var m = await _context.MonAn.Include(x => x.DanhMuc).FirstOrDefaultAsync(x => x.mon_id == id);
            
            if (m == null)
            {
                return NotFound("Không tìm thấy món ăn.");
            }

            return new MonAnDto
            {
                Id = m.mon_id,
                TenMon = m.ten_mon,
                Gia = m.gia,
                AnhUrl = m.anh_url,
                MoTa = m.mo_ta,
                CoSan = m.co_san,
                DanhMucId = m.danh_muc_id,
                TenDanhMuc = m.DanhMuc.ten_danh_muc
            };
        }

        // 3. THÊM MÓN MỚI (Admin, Manager)
        // POST: api/Menu
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<MonAnDto>> AddMonAn(CreateMonAnDto request)
        {
            // Kiểm tra danh mục có tồn tại không
            if (!await _context.DanhMuc.AnyAsync(d => d.danh_muc_id == request.DanhMucId))
            {
                return BadRequest("Danh mục không tồn tại.");
            }

            var monAn = new MonAn
            {
                ten_mon = request.TenMon,
                mo_ta = request.MoTa,
                gia = request.Gia,
                anh_url = request.AnhUrl,
                co_san = request.CoSan,
                danh_muc_id = request.DanhMucId
            };

            _context.MonAn.Add(monAn);
            await _context.SaveChangesAsync();

            // Lấy lại tên danh mục để trả về DTO đầy đủ
            var tenDanhMuc = await _context.DanhMuc
                .Where(d => d.danh_muc_id == request.DanhMucId)
                .Select(d => d.ten_danh_muc)
                .FirstOrDefaultAsync();

            var result = new MonAnDto
            {
                Id = monAn.mon_id,
                TenMon = monAn.ten_mon,
                Gia = monAn.gia,
                AnhUrl = monAn.anh_url,
                MoTa = monAn.mo_ta,
                CoSan = monAn.co_san,
                DanhMucId = monAn.danh_muc_id,
                TenDanhMuc = tenDanhMuc ?? ""
            };

            return CreatedAtAction(nameof(GetMonAn), new { id = monAn.mon_id }, result);
        }

        // 4. SỬA MÓN ĂN (Admin, Manager)
        // PUT: api/Menu/5
        // Sử dụng UpdateMonAnDto riêng biệt
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateMonAn(int id, UpdateMonAnDto request)
        {
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn để sửa.");
            }

            // Kiểm tra nếu đổi danh mục thì danh mục mới phải tồn tại
            if (monAn.danh_muc_id != request.DanhMucId)
            {
                 if (!await _context.DanhMuc.AnyAsync(d => d.danh_muc_id == request.DanhMucId))
                 {
                     return BadRequest("Danh mục mới không tồn tại.");
                 }
            }

            // Cập nhật thông tin
            monAn.ten_mon = request.TenMon;
            monAn.mo_ta = request.MoTa;
            monAn.gia = request.Gia;
            monAn.anh_url = request.AnhUrl;
            monAn.co_san = request.CoSan;
            monAn.danh_muc_id = request.DanhMucId;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật món ăn thành công.");
        }

        // 5. XÓA MÓN ĂN (Admin, Manager)
        // DELETE: api/Menu/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteMonAn(int id)
        {
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn == null)
            {
                return NotFound("Không tìm thấy món ăn.");
            }

            _context.MonAn.Remove(monAn);
            await _context.SaveChangesAsync();
            return Ok("Đã xóa món ăn.");
        }
    }
}