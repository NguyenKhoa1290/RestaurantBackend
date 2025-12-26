namespace RestaurantBackend.Models.Dtos
{
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; } // Nếu để null thì không đổi pass
    }
}