using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Models.Dtos;
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

        // ==================== ĐĂNG KÝ ====================
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserDto request)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest(new { message = "Tài khoản đã tồn tại." });
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    Role = "Manager"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đăng ký tài khoản Manager thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }

        // ==================== ĐĂNG NHẬP ====================
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserDto request)
        {
            try
            {
                Console.WriteLine($"🔍 Login attempt: {request.Username}");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    Console.WriteLine("❌ User not found");
                    return Unauthorized(new { message = "Sai tài khoản." });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    Console.WriteLine("❌ Wrong password");
                    return Unauthorized(new { message = "Sai mật khẩu." });
                }

                Console.WriteLine("✅ Login successful");

                string token = CreateToken(user);

                // ✅ TRẢ RESPONSE ĐÚNG FORMAT CHO WEBADMIN
                return Ok(new
                {
                    message = "Đăng nhập thành công",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }

        // ==================== TẠO TOKEN ====================
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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