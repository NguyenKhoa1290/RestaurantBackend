using RestaurantBackend.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace RestaurantBackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<MonAn> MonAn { get; set; }
        public DbSet<BanAn> BanAn { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
    }
}