using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    // 1. DTO Output: Dùng để trả dữ liệu ra Client (GET)
public class UpdateBanAnDto
    {
        [Required(ErrorMessage = "Số bàn/Tên bàn là bắt buộc")]
        public string SoBan { get; set; } = string.Empty;

        public string? MaQr { get; set; }

        public string TrangThai { get; set; } = "Trong";
    }
}