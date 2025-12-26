using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data; // Sửa lại namespace cho đúng project của bạn
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Models.Dtos;
namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // CHỈ ADMIN MỚI ĐƯỢC VÀO ĐÂY
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách tất cả Manager
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            // Chỉ lấy danh sách Manager, ẩn password hash đi cho bảo mật
            var users = await _context.Users
                .Where(u => u.Role == "Manager")
                .Select(u => new 
                {
                    u.Id,
                    u.Username,
                    u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. Admin tạo thêm Manager mới (Ngoài việc Manager tự đăng ký)
        [HttpPost]
        public async Task<ActionResult> CreateManager(UserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username đã tồn tại.");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Manager"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Đã tạo Manager thành công.");
        }

        // 3. Admin sửa thông tin Manager (Đổi tên, đổi pass)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManager(int id, UpdateUserDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy user.");

            // Không cho phép sửa tài khoản Admin khác (nếu có)
            if (user.Role == "Admin") return BadRequest("Không thể sửa tài khoản Admin.");

            user.Username = request.Username;

            // Nếu có nhập pass mới thì mới đổi
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công.");
        }

        // 4. Admin xóa Manager
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManager(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("User không tồn tại.");

            if (user.Role == "Admin") return BadRequest("Không thể xóa tài khoản Admin.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa User.");
        }
    }
}