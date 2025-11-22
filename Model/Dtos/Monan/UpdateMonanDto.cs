using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    public class UpdateMonAnDto
    {
        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        public string TenMon { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn hoặc bằng 0")]
        public decimal Gia { get; set; }

        public string? AnhUrl { get; set; }

        public bool CoSan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int DanhMucId { get; set; }
    }
}