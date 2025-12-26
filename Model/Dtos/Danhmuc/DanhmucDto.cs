using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. DTO Output: Dùng để trả dữ liệu ra cho Client xem (GET)
    public class DanhMucDto
    {
        public int Id { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int ThuTuHienThi { get; set; }
    }
}