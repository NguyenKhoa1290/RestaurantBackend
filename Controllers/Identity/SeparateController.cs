using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult GetPublic()
        {
            return Ok("Dữ liệu công khai.");
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được
        public IActionResult GetAdmin()
        {
            return Ok("Xin chào Admin! Bạn có quyền cao nhất.");
        }

        [HttpGet("user-only")]
        [Authorize(Roles = "Manager,Admin")] // User hoặc Admin đều vào được
        public IActionResult GetUser()
        {
            return Ok("Xin chào User! Bạn đã đăng nhập.");
        }
    }
}