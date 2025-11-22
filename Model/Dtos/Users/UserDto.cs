namespace RestaurantBackend.Models.Dtos
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Cho phép chọn Role khi đăng ký demo
    }
}