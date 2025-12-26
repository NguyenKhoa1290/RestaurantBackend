using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    public class CreateDonHangDto
    {
        [Required(ErrorMessage = "Phải chọn bàn ăn")]
        public int BanId { get; set; }

        public string SoBan { get; set; } = string.Empty;
        
        public string TenBan { get; set; } = string.Empty; // THÊM DÒNG NÀY
        
        public string? GhiChuKhach { get; set; }
        
        [Required(ErrorMessage = "Đơn hàng phải có ít nhất 1 món")]
        public List<CreateChiTietDonHangDto> MonOrder { get; set; } = new List<CreateChiTietDonHangDto>();
    }
}