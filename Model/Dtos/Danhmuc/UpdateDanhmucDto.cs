using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    public class UpdateDanhMucDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        public string TenDanhMuc { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public int ThuTuHienThi { get; set; }
    }
}