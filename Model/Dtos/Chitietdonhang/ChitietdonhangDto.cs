using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. Output: Hiển thị dòng món ăn trong hóa đơn (GET)
    public class ChiTietDonHangDto
    {
        public int Id { get; set; } // chi_tiet_id (để phục vụ sửa/xóa)
        public int MonId { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    // 2. Input: Dùng khi chọn món lúc tạo đơn (POST)
}