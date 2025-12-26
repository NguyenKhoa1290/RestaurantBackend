using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. Output (GET)
    public class DonHangDto
    {
        public int Id { get; set; }
        public string SoDon { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? GhiChuKhach { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        
        public int BanId { get; set; }
        public string SoBan { get; set; } = string.Empty;

        public List<ChiTietDonHangDto> ChiTiet { get; set; } = new List<ChiTietDonHangDto>();
    }
}