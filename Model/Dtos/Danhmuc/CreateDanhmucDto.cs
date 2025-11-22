using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    public class CreateDanhMucDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        public string TenDanhMuc { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        // Nếu không nhập, mặc định thứ tự là 0
        public int ThuTuHienThi { get; set; } = 0;
    }
}