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
    public class BananController : ControllerBase
    {
        private readonly DataContext _context;

        public BananController(DataContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH BÀN (Public)
        // GET: api/Table
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BanAnDto>>> GetTables()
        {
            return await _context.BanAn
                .OrderBy(b => b.so_ban) // Sắp xếp theo số bàn
                .Select(b => new BanAnDto
                {
                    Id = b.ban_id,
                    SoBan = b.so_ban,
                    MaQr = b.ma_qr,
                    TrangThai = b.trang_thai,
                    NgayTao = b.ngay_tao
                })
                .ToListAsync();
        }

        // 2. LẤY CHI TIẾT 1 BÀN (Public)
        // GET: api/Table/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BanAnDto>> GetTable(int id)
        {
            var b = await _context.BanAn.FindAsync(id);

            if (b == null)
            {
                return NotFound("Không tìm thấy bàn.");
            }

            return new BanAnDto
            {
                Id = b.ban_id,
                SoBan = b.so_ban,
                MaQr = b.ma_qr,
                TrangThai = b.trang_thai,
                NgayTao = b.ngay_tao
            };
        }

        // 3. THÊM BÀN MỚI (Admin, Manager)
        // POST: api/Table
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BanAnDto>> AddTable(CreateBanAnDto request)
        {
            // Kiểm tra trùng số bàn (Tùy chọn)
            if (await _context.BanAn.AnyAsync(b => b.so_ban == request.SoBan))
            {
                return BadRequest("Số bàn này đã tồn tại.");
            }

            var banAn = new BanAn
            {
                so_ban = request.SoBan,
                // Nếu không gửi mã QR, tự sinh mã ngẫu nhiên (Guid)
                ma_qr = request.MaQr ?? Guid.NewGuid().ToString(),
                trang_thai = request.TrangThai,
                ngay_tao = DateTime.Now
            };

            _context.BanAn.Add(banAn);
            await _context.SaveChangesAsync();

            var result = new BanAnDto
            {
                Id = banAn.ban_id,
                SoBan = banAn.so_ban,
                MaQr = banAn.ma_qr,
                TrangThai = banAn.trang_thai,
                NgayTao = banAn.ngay_tao
            };

            return CreatedAtAction(nameof(GetTable), new { id = banAn.ban_id }, result);
        }

        // 4. SỬA THÔNG TIN BÀN (Admin, Manager)
        // PUT: api/Table/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateTable(int id, UpdateBanAnDto request)
        {
            var banAn = await _context.BanAn.FindAsync(id);
            
            if (banAn == null)
            {
                return NotFound("Không tìm thấy bàn để sửa.");
            }

            // Cập nhật thông tin
            banAn.so_ban = request.SoBan;
            
            // Nếu gửi mã QR mới thì cập nhật, không thì giữ nguyên hoặc tùy logic
            if (!string.IsNullOrEmpty(request.MaQr))
            {
                banAn.ma_qr = request.MaQr;
            }
            
            banAn.trang_thai = request.TrangThai;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok("Cập nhật bàn thành công.");
        }

        // 5. XÓA BÀN (Admin, Manager)
        // DELETE: api/Table/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var banAn = await _context.BanAn.FindAsync(id);
            if (banAn == null)
            {
                return NotFound("Không tìm thấy bàn.");
            }

            // Kiểm tra ràng buộc: Không xóa bàn đang có đơn hàng (để bảo toàn lịch sử)
            // Hoặc chỉ chặn nếu bàn đang có đơn chưa thanh toán ("ChoXacNhan", "DangNau"...)
            bool dangCoDon = await _context.DonHang.AnyAsync(d => d.ban_id == id);
            if (dangCoDon)
            {
                // Tùy nghiệp vụ: Có thể cho xóa nhưng set null bên đơn hàng, hoặc chặn luôn.
                // Ở đây mình chặn cho an toàn.
                return BadRequest("Không thể xóa bàn này vì đã có lịch sử đơn hàng.");
            }

            _context.BanAn.Remove(banAn);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa bàn.");
        }
    }
}