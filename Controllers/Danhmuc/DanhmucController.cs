using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.Dtos;
using RestaurantBackend.Models.Entity; // Sử dụng namespace chứa DTO

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhmucController : ControllerBase
    {
        private readonly DataContext _context;

        public DanhmucController(DataContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH DANH MỤC (Public)
        // GET: api/Category
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DanhMucDto>>> GetDanhMucs()
        {
            // Chuyển đổi dữ liệu từ Entity sang DTO để trả về Client
            return await _context.DanhMuc
                .OrderBy(d => d.thu_tu_hien_thi)
                .Select(d => new DanhMucDto 
                {
                    Id = d.danh_muc_id,
                    TenDanhMuc = d.ten_danh_muc,
                    MoTa = d.mo_ta,
                    ThuTuHienThi = d.thu_tu_hien_thi
                })
                .ToListAsync();
        }

        // 2. LẤY CHI TIẾT 1 DANH MỤC (Public)
        // GET: api/Category/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DanhMucDto>> GetDanhMuc(int id)
        {
            var d = await _context.DanhMuc.FindAsync(id);

            if (d == null)
            {
                return NotFound("Không tìm thấy danh mục.");
            }

            return new DanhMucDto
            {
                Id = d.danh_muc_id,
                TenDanhMuc = d.ten_danh_muc,
                MoTa = d.mo_ta,
                ThuTuHienThi = d.thu_tu_hien_thi
            };
        }

        // 3. THÊM DANH MỤC MỚI (Chỉ Admin, Manager)
        // POST: api/Category
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DanhMucDto>> PostDanhMuc(CreateDanhMucDto request)
        {
            // Map từ DTO sang Entity để lưu vào DB
            var danhMuc = new DanhMuc
            {
                ten_danh_muc = request.TenDanhMuc,
                mo_ta = request.MoTa,
                thu_tu_hien_thi = request.ThuTuHienThi
            };

            _context.DanhMuc.Add(danhMuc);
            await _context.SaveChangesAsync();

            // Map ngược lại sang DTO để trả về kết quả
            var result = new DanhMucDto
            {
                Id = danhMuc.danh_muc_id,
                TenDanhMuc = danhMuc.ten_danh_muc,
                MoTa = danhMuc.mo_ta,
                ThuTuHienThi = danhMuc.thu_tu_hien_thi
            };

            return CreatedAtAction(nameof(GetDanhMuc), new { id = danhMuc.danh_muc_id }, result);
        }

        // 4. CẬP NHẬT DANH MỤC (Chỉ Admin, Manager)
        // PUT: api/Category/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutDanhMuc(int id, UpdateDanhMucDto request)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            
            if (danhMuc == null)
            {
                return NotFound("Không tìm thấy danh mục để sửa.");
            }

            // Cập nhật dữ liệu
            danhMuc.ten_danh_muc = request.TenDanhMuc;
            danhMuc.mo_ta = request.MoTa;
            danhMuc.thu_tu_hien_thi = request.ThuTuHienThi;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok("Cập nhật danh mục thành công.");
        }

        // 5. XÓA DANH MỤC (Chỉ Admin, Manager)
        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteDanhMuc(int id)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc == null)
            {
                return NotFound("Không tìm thấy danh mục.");
            }

            // Kiểm tra ràng buộc: Nếu danh mục đang có món ăn thì không cho xóa
            if (await _context.MonAn.AnyAsync(m => m.danh_muc_id == id))
            {
                return BadRequest("Không thể xóa danh mục này vì đang chứa món ăn. Vui lòng xóa hết món ăn trước.");
            }

            _context.DanhMuc.Remove(danhMuc);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa danh mục.");
        }
    }
}