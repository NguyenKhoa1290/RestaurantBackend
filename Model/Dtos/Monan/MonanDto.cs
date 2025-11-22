using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. Output: Dùng để hiển thị (GET)
    public class MonAnDto
    {
        public int Id { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public string? AnhUrl { get; set; }
        public string? MoTa { get; set; }
        public bool CoSan { get; set; }
        
        // Flatten: Lấy tên danh mục để hiển thị luôn
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
    }
}