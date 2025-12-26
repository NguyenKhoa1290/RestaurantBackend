using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // ✅ THÊM
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Dtos;
using RestaurantBackend.Hubs; // ✅ THÊM

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonhangController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<OrderHub> _hubContext; // ✅ THÊM
        private readonly HttpClient _httpClient; // ✅ THÊM

        // ✅ Inject IHubContext và HttpClient vào constructor
        public DonhangController(DataContext context, IHubContext<OrderHub> hubContext, HttpClient httpClient)
        {
            _context = context;
            _hubContext = hubContext;
            _httpClient = httpClient;
        }

        // 1. LẤY DANH SÁCH ĐƠN HÀNG (GET)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DonHangDto>>> GetOrders()
        {
            var orders = await _context.DonHang
                .Include(d => d.BanAn)
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.MonAn)
                .OrderByDescending(d => d.ngay_tao)
                .ToListAsync();

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
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<DonHangDto>> CreateOrder(CreateDonHangDto request)
        {
            // 3.1. Kiểm tra bàn có tồn tại không
            var banAn = await _context.BanAn.FindAsync(request.BanId);
            
            if (banAn == null && !string.IsNullOrEmpty(request.SoBan))
            {
                banAn = await _context.BanAn
                    .FirstOrDefaultAsync(b => b.so_ban == request.SoBan || 
                                             b.so_ban == $"Bàn {request.SoBan}");
            }
            
            if (banAn == null) 
            {
                var errorMessage = !string.IsNullOrEmpty(request.SoBan) 
                    ? $"Bàn ăn {request.SoBan} không tồn tại." 
                    : "Bàn ăn không tồn tại.";
                return BadRequest(errorMessage);
            }

            // 3.2. Khởi tạo đơn hàng
            var donHang = new DonHang
            {
                ban_id = banAn.ban_id,
                so_don = $"ORD-{DateTime.Now:yyMMddHHmmss}",
                ghi_chu_khach = FormatGhiChu(request.GhiChuKhach, request.TenBan, banAn.so_ban),
                trang_thai = "ChoXacNhan",
                ngay_tao = DateTime.Now,
                ChiTietDonHang = new List<ChiTietDonHang>()
            };
            
            decimal tongTienTamTinh = 0;

            // 3.3. Duyệt qua từng món khách chọn
            foreach (var item in request.MonOrder)
            {
                var monAn = await _context.MonAn.FindAsync(item.MonId);
                if (monAn == null)
                {
                    return BadRequest($"Món ăn có ID {item.MonId} không tồn tại.");
                }

                var chiTiet = new ChiTietDonHang
                {
                    mon_id = item.MonId,
                    so_luong = item.SoLuong,
                    don_gia = monAn.gia,
                    thanh_tien = monAn.gia * item.SoLuong
                };

                tongTienTamTinh += chiTiet.thanh_tien;
                donHang.ChiTietDonHang.Add(chiTiet);
            }

            // 3.4. Gán tổng tiền và Lưu vào DB
            donHang.tong_tien = tongTienTamTinh;
            donHang.ngay_cap_nhat = DateTime.Now;

            _context.DonHang.Add(donHang);
            await _context.SaveChangesAsync();

            // ✅ 3.5. GỌI ORDER MICROSERVICE ĐỂ LƯU VÀO ORDERMICROSERVICEDB
            try
            {
                var orderMicroserviceDto = new
                {
                    tableId = donHang.ban_id,
                    tableName = banAn.so_ban,
                    customerNote = donHang.ghi_chu_khach,
                    items = donHang.ChiTietDonHang.Select(ct => new
                    {
                        dishId = ct.mon_id,
                        dishName = _context.MonAn.Find(ct.mon_id)?.ten_mon,
                        quantity = ct.so_luong,
                        unitPrice = ct.don_gia,
                        dishNote = ""
                    }).ToList()
                };

                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(orderMicroserviceDto),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://service.cachephoarong.click/api/orders", jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Order synced to OrderMicroservice: Order #{donHang.so_don}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to sync order to OrderMicroservice: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ OrderMicroservice sync failed: {ex.Message}");
            }

            // ✅ 3.6. BROADCAST ĐƠN HÀNG MỚI QUA SIGNALR
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNewOrder", new
                {
                    orderId = donHang.don_id,
                    soDon = donHang.so_don,
                    banId = donHang.ban_id,
                    soBan = banAn.so_ban,
                    tongTien = donHang.tong_tien,
                    trangThai = donHang.trang_thai,
                    ghiChu = donHang.ghi_chu_khach,
                    ngayTao = donHang.ngay_tao,
                    chiTiet = donHang.ChiTietDonHang.Select(ct => new
                    {
                        monId = ct.mon_id,
                        tenMon = _context.MonAn.Find(ct.mon_id)?.ten_mon,
                        soLuong = ct.so_luong,
                        donGia = ct.don_gia,
                        thanhTien = ct.thanh_tien
                    }).ToList()
                });
                
                Console.WriteLine($"📡 Broadcasted ReceiveNewOrder: Order #{donHang.so_don} - Bàn {banAn.so_ban}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SignalR broadcast failed: {ex.Message}");
            }

            // 3.7. Trả về kết quả
            return CreatedAtAction(nameof(GetOrder), new { id = donHang.don_id }, 
                new { 
                    id = donHang.don_id, 
                    soDon = donHang.so_don,
                    ban = banAn.so_ban,
                    tongTien = donHang.tong_tien,
                    msg = "Đặt món thành công" 
                });
        }

        // Helper method để format ghi chú
        private string FormatGhiChu(string ghiChuKhach, string tenBan, string soBan)
        {
            // ✅ CHỈ TRẢ VỀ GHI CHÚ CỦA KHÁCH, KHÔNG THÊM THÔNG TIN BÀN
            // (Vì UI đã hiển thị số bàn ở cột riêng rồi)
            
            if (!string.IsNullOrEmpty(ghiChuKhach))
            {
                return ghiChuKhach.Trim();
            }
            
            return string.Empty;
        }

        // 4. CẬP NHẬT ĐƠN HÀNG (PUT)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateDonHangDto request)
        {
            var donHang = await _context.DonHang
                .Include(d => d.BanAn) // ✅ Include để lấy số bàn
                .FirstOrDefaultAsync(d => d.don_id == id);
                
            if (donHang == null) return NotFound("Không tìm thấy đơn hàng.");

            // Lưu trạng thái cũ để so sánh
            var oldStatus = donHang.trang_thai;

            // 1. Xử lý chuyển bàn
            if (donHang.ban_id != request.BanId)
            {
                 if (!await _context.BanAn.AnyAsync(b => b.ban_id == request.BanId))
                 {
                     return BadRequest("Bàn ăn mới không tồn tại.");
                 }
                 donHang.ban_id = request.BanId;
            }

            // 2. Cập nhật Mã số đơn
            if (!string.IsNullOrEmpty(request.SoDon))
            {
                donHang.so_don = request.SoDon;
            }

            // 3. Cập nhật các thông tin khác
            donHang.tong_tien = request.TongTien;
            donHang.trang_thai = request.TrangThai;
            donHang.ghi_chu_khach = request.GhiChuKhach;
            donHang.ngay_cap_nhat = request.NgayCapNhat ?? DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ 4. BROADCAST THAY ĐỔI TRẠNG THÁI QUA SIGNALR
            // Chỉ broadcast nếu trạng thái thực sự thay đổi
            if (oldStatus != request.TrangThai)
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("OrderStatusChanged", new
                    {
                        orderId = donHang.don_id,
                        soDon = donHang.so_don,
                        banId = donHang.ban_id,
                        soBan = donHang.BanAn?.so_ban,
                        oldStatus = oldStatus,
                        newStatus = request.TrangThai,
                        ngayCapNhat = donHang.ngay_cap_nhat
                    });
                    
                    Console.WriteLine($"📡 Broadcasted OrderStatusChanged: Order #{donHang.so_don} - {oldStatus} → {request.TrangThai}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ SignalR broadcast failed: {ex.Message}");
                }
            }

            return Ok("Cập nhật đơn hàng thành công.");
        }

        // 5. XÓA ĐƠN HÀNG (DELETE)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            if (donHang == null) return NotFound();

            _context.DonHang.Remove(donHang);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa đơn hàng.");
        }
    }
}