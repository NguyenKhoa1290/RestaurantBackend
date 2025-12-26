using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. DTO Output: Dùng để trả dữ liệu ra Client (GET)
    public class BanAnDto
    {
        public int Id { get; set; }
        public string SoBan { get; set; } = string.Empty;
        public string? MaQr { get; set; }
        public string TrangThai { get; set; } = "Trong"; // Ví dụ: Trong, CoKhach
        public DateTime NgayTao { get; set; }
    }
}