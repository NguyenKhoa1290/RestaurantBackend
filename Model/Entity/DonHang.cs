using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestaurantBackend.Models.Entity
{
        public class DonHang
    {
        [Key]
        public int don_id { get; set; }

        // KHÓA NGOẠI: Liên kết với BanAn
        public int ban_id { get; set; }
        [ForeignKey("ban_id")]
        public BanAn BanAn { get; set; }

        public string so_don { get; set; } = string.Empty; // Mã hiển thị, vd: #DH001
        public decimal tong_tien { get; set; }
        public string trang_thai { get; set; } = "ChoXacNhan";
        public string? ghi_chu_khach { get; set; }
        public DateTime ngay_tao { get; set; } = DateTime.Now;
        public DateTime? ngay_cap_nhat { get; set; }

        // Quan hệ: 1 Đơn hàng có nhiều Chi tiết
        public ICollection<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}