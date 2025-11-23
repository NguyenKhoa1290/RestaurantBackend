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
    public class DonhangController : ControllerBase
    {
        private readonly DataContext _context;

        public DonhangController(DataContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH ĐƠN HÀNG (GET)
        // GET: api/Order
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DonHangDto>>> GetOrders()
        {
            // Load đơn hàng kèm thông tin Bàn và Chi tiết món
            var orders = await _context.DonHang
                .Include(d => d.BanAn)
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.MonAn) // Load tiếp thông tin Món để lấy tên
                .OrderByDescending(d => d.ngay_tao) // Đơn mới nhất lên đầu
                .ToListAsync();

            // Map sang DTO
            var result = orders.Select(d => new DonHangDto
            {
                Id = d.don_id,
                SoDon = d.so_don,
                TongTien = d.tong_tien,
                TrangThai = d.trang_thai,
                GhiChuKhach = d.ghi_chu_khach,
                NgayTao = d.ngay_tao,
                NgayCapNhat = d.ngay_cap_nhat,
                BanId = d.ban_id,
                SoBan = d.BanAn.so_ban,
                // Map danh sách chi tiết món
                ChiTiet = d.ChiTietDonHang.Select(ct => new ChiTietDonHangDto
                {
                    Id = ct.chi_tiet_id,
                    MonId = ct.mon_id,
                    TenMon = ct.MonAn.ten_mon,
                    SoLuong = ct.so_luong,
                    DonGia = ct.don_gia,
                    ThanhTien = ct.thanh_tien
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // 2. LẤY CHI TIẾT 1 ĐƠN HÀNG (GET)
        // GET: api/Order/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DonHangDto>> GetOrder(int id)
        {
            var d = await _context.DonHang
                .Include(x => x.BanAn)
                .Include(x => x.ChiTietDonHang).ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(x => x.don_id == id);

            if (d == null) return NotFound("Không tìm thấy đơn hàng.");

            return new DonHangDto
            {
                Id = d.don_id,
                SoDon = d.so_don,
                TongTien = d.tong_tien,
                TrangThai = d.trang_thai,
                GhiChuKhach = d.ghi_chu_khach,
                NgayTao = d.ngay_tao,
                NgayCapNhat = d.ngay_cap_nhat,
                BanId = d.ban_id,
                SoBan = d.BanAn.so_ban,
                ChiTiet = d.ChiTietDonHang.Select(ct => new ChiTietDonHangDto
                {
                    Id = ct.chi_tiet_id,
                    MonId = ct.mon_id,
                    TenMon = ct.MonAn.ten_mon,
                    SoLuong = ct.so_luong,
                    DonGia = ct.don_gia,
                    ThanhTien = ct.thanh_tien
                }).ToList()
            };
        }

        // 3. TẠO ĐƠN HÀNG MỚI (POST)
        // POST: api/Order
        [HttpPost]
        [AllowAnonymous] // Khách có thể tự gọi hoặc nhân viên gọi giúp
        public async Task<ActionResult<DonHangDto>> CreateOrder(CreateDonHangDto request)
        {
            // 3.1. Kiểm tra bàn có tồn tại không
            var banAn = await _context.BanAn.FindAsync(request.BanId);
            if (banAn == null) return BadRequest("Bàn ăn không tồn tại.");

            // 3.2. Khởi tạo đơn hàng
            var donHang = new DonHang
            {
                ban_id = request.BanId,
                so_don = $"ORD-{DateTime.Now:HHmmss}", // Mã đơn tự sinh: ORD-123045
                ghi_chu_khach = request.GhiChuKhach,
                trang_thai = "ChoXacNhan",
                ngay_tao = DateTime.Now,
                ChiTietDonHang = new List<ChiTietDonHang>()
            };

            decimal tongTienTamTinh = 0;

            // 3.3. Duyệt qua từng món khách chọn để lấy giá & tính tiền
            foreach (var item in request.MonOrder)
            {
                var monAn = await _context.MonAn.FindAsync(item.MonId);
                if (monAn == null)
                {
                    return BadRequest($"Món ăn có ID {item.MonId} không tồn tại.");
                }

                // Tạo chi tiết đơn
                var chiTiet = new ChiTietDonHang
                {
                    mon_id = item.MonId,
                    so_luong = item.SoLuong,
                    don_gia = monAn.gia, // Lấy giá gốc từ DB (An toàn)
                    thanh_tien = monAn.gia * item.SoLuong
                };

                // Cộng dồn tổng tiền
                tongTienTamTinh += chiTiet.thanh_tien;

                // Thêm vào list chi tiết của đơn
                donHang.ChiTietDonHang.Add(chiTiet);
            }

            // 3.4. Gán tổng tiền và Lưu vào DB
            donHang.tong_tien = tongTienTamTinh;

            _context.DonHang.Add(donHang);
            await _context.SaveChangesAsync(); // EF Core sẽ tự lưu cả Đơn + Chi tiết

            // 3.5. Trả về kết quả
            return CreatedAtAction(nameof(GetOrder), new { id = donHang.don_id }, new { id = donHang.don_id, msg = "Đặt món thành công" });
        }

        // 4. CẬP NHẬT ĐƠN HÀNG (PUT) - Sửa trạng thái, chuyển bàn, sửa tiền...
        // PUT: api/Order/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateDonHangDto request)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            if (donHang == null) return NotFound("Không tìm thấy đơn hàng.");

            // 1. Xử lý chuyển bàn (nếu ID bàn thay đổi)
            if (donHang.ban_id != request.BanId)
            {
                 if (!await _context.BanAn.AnyAsync(b => b.ban_id == request.BanId))
                 {
                     return BadRequest("Bàn ăn mới không tồn tại.");
                 }
                 donHang.ban_id = request.BanId;
            }

            // 2. Cập nhật Mã số đơn (nếu có gửi lên)
            if (!string.IsNullOrEmpty(request.SoDon))
            {
                donHang.so_don = request.SoDon;
            }

            // 3. Cập nhật các thông tin khác
            donHang.tong_tien = request.TongTien; // Cho phép sửa tay tổng tiền
            donHang.trang_thai = request.TrangThai;
            donHang.ghi_chu_khach = request.GhiChuKhach;
            
            // Nếu không gửi ngày cập nhật lên thì lấy giờ hiện tại
            donHang.ngay_cap_nhat = request.NgayCapNhat ?? DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật đơn hàng thành công.");
        }

        // 5. XÓA ĐƠN HÀNG (DELETE)
        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa đơn
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            if (donHang == null) return NotFound();

            // Khi xóa đơn hàng, EF Core sẽ tự động xóa các ChiTietDonHang liên quan 
            // (nếu cấu hình Cascade Delete mặc định của DB là ON, hoặc EF tự xử lý)
            _context.DonHang.Remove(donHang);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa đơn hàng.");
        }
    }
}