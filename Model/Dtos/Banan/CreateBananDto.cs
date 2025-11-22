using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. DTO Output: Dùng để trả dữ liệu ra Client (GET)
public class CreateBanAnDto
    {
        [Required(ErrorMessage = "Số bàn/Tên bàn là bắt buộc")]
        public string SoBan { get; set; } = string.Empty;
        
        public string? MaQr { get; set; } // Có thể để null để hệ thống tự sinh
        
        public string TrangThai { get; set; } = "Trong";
    }
}