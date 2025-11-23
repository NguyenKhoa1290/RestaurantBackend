using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity; // Thêm namespace này để dùng được class User
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ DỊCH VỤ (SERVICES) ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger để nhập Token (Tự động thêm Bearer)
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Chỉ cần paste Token vào ô bên dưới (không cần gõ chữ Bearer)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Kết nối Database SQLite
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Xác thực (Bạn là ai?)
app.UseAuthorization();  // Phân quyền (Bạn được làm gì?)

app.MapControllers();

// --- 3. TỰ ĐỘNG TẠO TÀI KHOẢN ADMIN (SEED DATA) ---
// Đoạn code này chạy mỗi khi khởi động server để đảm bảo luôn có Admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        
        // Nếu chưa có user nào là Admin thì tạo mới
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            Console.WriteLine("--> Đang tạo tài khoản Admin mặc định...");
            
            var adminUser = new User
            {
                Username = "admin",
                // Mật khẩu mặc định: admin123
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
            
            Console.WriteLine("--> Đã tạo xong Admin: User='admin', Pass='admin123'");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> Lỗi khi tạo Admin: " + ex.Message);
    }
}

app.Run();