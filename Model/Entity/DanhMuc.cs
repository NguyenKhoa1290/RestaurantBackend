using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RestaurantBackend.Models.Entity
{
    // 1. Bảng Danh Mục
    public class DanhMuc
    {
        [Key]
        public int danh_muc_id { get; set; }
        public string ten_danh_muc { get; set; } = string.Empty;
        public string? mo_ta { get; set; }
        public int thu_tu_hien_thi { get; set; }

        // Quan hệ: 1 DanhMuc có nhiều MonAn
        public ICollection<MonAn> MonAn { get; set; }
    }
}