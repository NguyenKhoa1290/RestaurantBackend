using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestaurantBackend.Models.Entity
{
    // 2. Bảng Món Ăn
public class MonAn
    {
        [Key]
        public int mon_id { get; set; }

        // KHÓA NGOẠI: Liên kết với DanhMuc
        public int danh_muc_id { get; set; }
        [ForeignKey("danh_muc_id")]
        public DanhMuc DanhMuc { get; set; }

        public string ten_mon { get; set; } = string.Empty;
        public string? mo_ta { get; set; }
        public decimal gia { get; set; }
        public string? anh_url { get; set; }
        public bool co_san { get; set; }
        public DateTime ngay_tao { get; set; } = DateTime.Now;

        // Quan hệ: 1 MonAn nằm trong nhiều ChiTietDonHang
        public ICollection<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}