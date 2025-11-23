using RestaurantBackend.Data; // Hoặc RestaurantBackend.Data tùy namespace của bạn
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Models.Dtos; // Hoặc RestaurantBackend.Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Đăng ký (Ai cũng gọi được -> Mặc định tạo ra Manager)
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Tài khoản đã tồn tại.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "Manager" // <--- GÁN CỨNG ROLE LÀ MANAGER
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký tài khoản Manager thành công!");
        }

        // Đăng nhập (Giữ nguyên logic cũ)
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null) return BadRequest("Sai tài khoản.");
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return BadRequest("Sai mật khẩu.");

            string token = CreateToken(user);
            return Ok(token);
        }

        // Hàm tạo Token (Giữ nguyên)
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}