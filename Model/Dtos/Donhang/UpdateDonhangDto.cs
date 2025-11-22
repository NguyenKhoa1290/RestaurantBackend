using System.ComponentModel.DataAnnotations;

namespace RestaurantBackend.Dtos
{
    public class UpdateDonHangDto
    {
        public string? SoDon { get; set; } // Cho phép sửa lại mã đơn (VD: ORD-ERROR -> ORD-FIXED)

        [Required(ErrorMessage = "Phải chọn bàn ăn")]
        public int BanId { get; set; } 

        public decimal TongTien { get; set; } 

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string TrangThai { get; set; } = string.Empty;

        public string? GhiChuKhach { get; set; }

        public DateTime? NgayCapNhat { get; set; } // Cho phép chỉnh tay ngày cập nhật (nếu muốn)
    }
}