using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestaurantBackend.Models.Entity
{
public class ChiTietDonHang
    {
        [Key]
        public int chi_tiet_id { get; set; }

        // KHÓA NGOẠI 1: Liên kết với DonHang
        public int don_id { get; set; }
        [ForeignKey("don_id")]
        public DonHang DonHang { get; set; }

        // KHÓA NGOẠI 2: Liên kết với MonAn
        public int mon_id { get; set; }
        [ForeignKey("mon_id")]
        public MonAn MonAn { get; set; }

        public int so_luong { get; set; }
        public decimal don_gia { get; set; } // Lưu giá tại thời điểm đặt
        public decimal thanh_tien { get; set; }
    }
}