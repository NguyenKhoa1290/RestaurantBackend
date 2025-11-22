using System.ComponentModel.DataAnnotations;
namespace RestaurantBackend.Dtos
{
public class CreateChiTietDonHangDto
    {
        [Required(ErrorMessage = "Phải chọn món ăn")]
        public int MonId { get; set; }
        
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int SoLuong { get; set; }
    }
}

    // 3. Input: Dùng khi sửa số lượng món (PUT)