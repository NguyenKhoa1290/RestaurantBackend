using RestaurantBackend.Data;
using RestaurantBackend.Models.Entity;
using RestaurantBackend.Hubs; // ✅ THÊM DÒNG NÀY
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ DỊCH VỤ (SERVICES) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ THÊM: HttpClient cho gọi OrderMicroservice
builder.Services.AddHttpClient();

// ✅ THÊM: SignalR
builder.Services.AddSignalR();

// CORS - ⚠️ CẬP NHẬT: Cần AllowCredentials cho SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://admin.cachephoarong.click",  // admin
                "http://localhost:5137",  // server
                "https://cachephoarong.click",  // client
                "http://127.0.0.1:5192",
                "http://127.0.0.1:5137",
                "http://127.0.0.1:5002"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // ⚠️ QUAN TRỌNG: SignalR cần credentials
    });
});

// Swagger
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

// Database
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("AppSettings:Token");
var secretKey = jwtSettings.Value;

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("❌ AppSettings:Token không được cấu hình trong appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ QUAN TRỌNG: Static Files với CORS
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
        ctx.Context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
    }
});

// ⚠️ QUAN TRỌNG: CORS phải đặt TRƯỚC MapHub
app.UseCors("AllowAll");

// Comment HTTPS redirect khi dev
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ THÊM: Map SignalR Hub
app.MapHub<OrderHub>("/orderHub");

// --- 3. SEED ADMIN ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();

        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            Console.WriteLine("--> Đang tạo tài khoản Admin mặc định...");

            var adminUser = new User
            {
                Username = "admin",
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

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("\n✅ API JWT đang chạy - Sẵn sàng nhận request!");
    Console.WriteLine($"📁 Static files path: {app.Environment.WebRootPath}");
    Console.WriteLine($"🔌 SignalR Hub endpoint: /orderHub"); // ✅ THÊM LOG
    Console.WriteLine();
}

app.Run();