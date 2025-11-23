using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Dtos;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChitietdonhangController : ControllerBase
    {
        private readonly DataContext _context;

        public ChitietdonhangController(DataContext context)
        {
            _context = context;
        }

        // 1. THÊM MÓN VÀO ĐƠN HÀNG ĐÃ CÓ (POST)
        // URL: POST /api Chitietdonhang/add-to-order/{donId}
        // Dùng khi khách đang ăn mà muốn gọi thêm món mới vào đơn cũ
        [HttpPost("add-to-order/{donId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddItemToOrder(int donId, CreateChiTietDonHangDto request)
        {
            // Kiểm tra đơn hàng có tồn tại không
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .FirstOrDefaultAsync(d => d.don_id == donId);

            if (donHang == null) return NotFound("Đơn hàng không tồn tại.");

            // Kiểm tra món ăn có tồn tại không
            var monAn = await _context.MonAn.FindAsync(request.MonId);
            if (monAn == null) return BadRequest("Món ăn không tồn tại.");

            // Kiểm tra xem món này đã có trong đơn chưa
            var chiTietTonTai = donHang.ChiTietDonHang.FirstOrDefault(x => x.mon_id == request.MonId);

            if (chiTietTonTai != null)
            {
                // CASE 1: Món đã có -> Cộng dồn số lượng và tính lại tiền dòng đó
                // Ví dụ: Đang có 2 bia, gọi thêm 3 bia -> Thành 5 bia
                chiTietTonTai.so_luong += request.SoLuong;
                chiTietTonTai.thanh_tien = chiTietTonTai.so_luong * chiTietTonTai.don_gia;
            }
            else
            {
                // CASE 2: Món chưa có -> Tạo dòng chi tiết mới
                var chiTietMoi = new ChiTietDonHang
                {
                    don_id = donId,
                    mon_id = request.MonId,
                    so_luong = request.SoLuong,
                    don_gia = monAn.gia, // Lấy giá gốc tại thời điểm gọi
                    thanh_tien = request.SoLuong * monAn.gia
                };
                _context.ChiTietDonHang.Add(chiTietMoi);
            }

            // QUAN TRỌNG: Cộng thêm tiền của món vừa gọi vào tổng tiền đơn hàng
            donHang.tong_tien += (request.SoLuong * monAn.gia);
            donHang.ngay_cap_nhat = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Đã thêm món vào đơn hàng.");
        }

        // 2. SỬA SỐ LƯỢNG MÓN (PUT)
        // URL: PUT /api Chitietdonhang/{id} (ID này là chi_tiet_id)
        // Dùng khi nhân viên nhập sai số lượng hoặc khách đổi ý (VD: 5 bia -> giảm còn 3 bia)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateQuantity(int id, UpdateChiTietDonHangDto request)
        {
            // Load chi tiết kèm theo thông tin Đơn hàng cha để sửa tổng tiền
            var chiTiet = await _context.ChiTietDonHang
                .Include(ct => ct.DonHang)
                .FirstOrDefaultAsync(c => c.chi_tiet_id == id);

            if (chiTiet == null) return NotFound("Không tìm thấy dòng chi tiết này.");

            // Lưu lại tiền cũ để trừ ra
            decimal tienCu = chiTiet.thanh_tien;
            
            // Tính tiền mới
            decimal tienMoi = request.SoLuong * chiTiet.don_gia;

            // Cập nhật thông tin dòng chi tiết
            chiTiet.so_luong = request.SoLuong;
            chiTiet.thanh_tien = tienMoi;

            // Cập nhật tổng tiền đơn hàng cha (Logic: Tổng = Tổng cũ - Tiền dòng cũ + Tiền dòng mới)
            chiTiet.DonHang.tong_tien = chiTiet.DonHang.tong_tien - tienCu + tienMoi;
            chiTiet.DonHang.ngay_cap_nhat = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Đã cập nhật số lượng món.");
        }

        // 3. XÓA MÓN KHỎI ĐƠN (DELETE)
        // URL: DELETE /api Chitietdonhang/{id}
        // Dùng khi khách hủy món hoặc nhân viên nhập nhầm hẳn món đó
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var chiTiet = await _context.ChiTietDonHang
                .Include(ct => ct.DonHang)
                .FirstOrDefaultAsync(c => c.chi_tiet_id == id);

            if (chiTiet == null) return NotFound("Không tìm thấy dòng chi tiết này.");

            // Trừ tiền món này khỏi tổng đơn hàng
            chiTiet.DonHang.tong_tien -= chiTiet.thanh_tien;
            chiTiet.DonHang.ngay_cap_nhat = DateTime.Now;

            // Xóa dòng chi tiết khỏi DB
            _context.ChiTietDonHang.Remove(chiTiet);

            await _context.SaveChangesAsync();
            return Ok("Đã xóa món khỏi đơn hàng.");
        }
    }
}