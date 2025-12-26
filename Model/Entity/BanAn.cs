using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestaurantBackend.Models.Entity
{
    public class BanAn
    {
        [Key]
        public int ban_id { get; set; }
        public string so_ban { get; set; } = string.Empty;
        public string? ma_qr { get; set; }
        public string trang_thai { get; set; } = "Trong"; // Ví dụ: Trống, Có khách
        public DateTime ngay_tao { get; set; } = DateTime.Now;

        // Quan hệ: 1 Bàn có nhiều Đơn hàng
        public ICollection<DonHang> DonHang { get; set; }
    }
}
