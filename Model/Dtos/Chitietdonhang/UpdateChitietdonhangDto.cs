using System.ComponentModel.DataAnnotations;
namespace RestaurantBackend.Dtos
{
    // 3. Input: Dùng khi sửa số lượng món (PUT)
    public class UpdateChiTietDonHangDto
    {
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int SoLuong { get; set; }
    }
}