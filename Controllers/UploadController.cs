using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace RestaurantBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;
        
        // Cấu hình
        private const long MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB
        private const int TARGET_WIDTH = 800;
        private const int TARGET_HEIGHT = 600;
        private readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png" };

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Upload ảnh món ăn - Resize về 800x600px
        /// </summary>
        [HttpPost("image")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                // 1. VALIDATE FILE
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn file ảnh" });
                }

                // Kiểm tra kích thước
                if (file.Length > MAX_FILE_SIZE)
                {
                    return BadRequest(new { message = "File ảnh không được vượt quá 2MB" });
                }

                // Kiểm tra extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!ALLOWED_EXTENSIONS.Contains(extension))
                {
                    return BadRequest(new { message = "Chỉ chấp nhận file JPG, JPEG, PNG" });
                }

                // 2. TẠO TÊN FILE MỚI (timestamp + tên gốc)
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                var safeFileName = string.Concat(originalFileName.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
                var newFileName = $"{timestamp}_{safeFileName}.jpg";

                // 3. TẠO ĐƯỜNG DẪN LƯU FILE
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "dishes");
                
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation("✅ Created directory: {Path}", uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, newFileName);

                // 4. XỬ LÝ ẢNH: RESIZE + COMPRESS
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    // Resize về 800x600 (crop để giữ tỉ lệ 4:3)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(TARGET_WIDTH, TARGET_HEIGHT),
                        Mode = ResizeMode.Crop
                    }));

                    // Lưu với quality 85%
                    var encoder = new JpegEncoder { Quality = 85 };
                    await image.SaveAsync(filePath, encoder);
                }

                // 5. TẠO URL TRUY CẬP
                var imageUrl = $"/uploads/dishes/{newFileName}";

                _logger.LogInformation("✅ Uploaded image: {FileName} → {Url}", file.FileName, imageUrl);

                return Ok(new
                {
                    success = true,
                    message = "Upload ảnh thành công",
                    url = imageUrl,
                    fileName = newFileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Upload error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Lỗi khi upload ảnh: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa ảnh (dùng khi update món ăn)
        /// </summary>
        [HttpDelete("image")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteImage([FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest(new { message = "URL ảnh không được để trống" });
                }

                // Lấy tên file từ URL (ví dụ: /uploads/dishes/abc.jpg → abc.jpg)
                var fileName = Path.GetFileName(url);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "dishes", fileName);

                // Xóa file nếu tồn tại
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("✅ Deleted image: {FileName}", fileName);
                    return Ok(new { message = "Xóa ảnh thành công" });
                }

                return NotFound(new { message = "Không tìm thấy file ảnh" });
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Delete error: {Message}", ex.Message);
                return StatusCode(500, new { message = "Lỗi khi xóa ảnh: " + ex.Message });
            }
        }
    }
}