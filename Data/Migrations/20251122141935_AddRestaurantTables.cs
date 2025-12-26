using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant_backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanAn",
                columns: table => new
                {
                    ban_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    so_ban = table.Column<string>(type: "TEXT", nullable: false),
                    ma_qr = table.Column<string>(type: "TEXT", nullable: true),
                    trang_thai = table.Column<string>(type: "TEXT", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanAn", x => x.ban_id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    danh_muc_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ten_danh_muc = table.Column<string>(type: "TEXT", nullable: false),
                    mo_ta = table.Column<string>(type: "TEXT", nullable: true),
                    thu_tu_hien_thi = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.danh_muc_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    don_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ban_id = table.Column<int>(type: "INTEGER", nullable: false),
                    so_don = table.Column<string>(type: "TEXT", nullable: false),
                    tong_tien = table.Column<decimal>(type: "TEXT", nullable: false),
                    trang_thai = table.Column<string>(type: "TEXT", nullable: false),
                    ghi_chu_khach = table.Column<string>(type: "TEXT", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.don_id);
                    table.ForeignKey(
                        name: "FK_DonHang_BanAn_ban_id",
                        column: x => x.ban_id,
                        principalTable: "BanAn",
                        principalColumn: "ban_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonAn",
                columns: table => new
                {
                    mon_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    danh_muc_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ten_mon = table.Column<string>(type: "TEXT", nullable: false),
                    mo_ta = table.Column<string>(type: "TEXT", nullable: true),
                    gia = table.Column<decimal>(type: "TEXT", nullable: false),
                    anh_url = table.Column<string>(type: "TEXT", nullable: true),
                    co_san = table.Column<bool>(type: "INTEGER", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonAn", x => x.mon_id);
                    table.ForeignKey(
                        name: "FK_MonAn_DanhMuc_danh_muc_id",
                        column: x => x.danh_muc_id,
                        principalTable: "DanhMuc",
                        principalColumn: "danh_muc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    chi_tiet_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    don_id = table.Column<int>(type: "INTEGER", nullable: false),
                    mon_id = table.Column<int>(type: "INTEGER", nullable: false),
                    so_luong = table.Column<int>(type: "INTEGER", nullable: false),
                    don_gia = table.Column<decimal>(type: "TEXT", nullable: false),
                    thanh_tien = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => x.chi_tiet_id);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_don_id",
                        column: x => x.don_id,
                        principalTable: "DonHang",
                        principalColumn: "don_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_MonAn_mon_id",
                        column: x => x.mon_id,
                        principalTable: "MonAn",
                        principalColumn: "mon_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_don_id",
                table: "ChiTietDonHang",
                column: "don_id");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_mon_id",
                table: "ChiTietDonHang",
                column: "mon_id");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ban_id",
                table: "DonHang",
                column: "ban_id");

            migrationBuilder.CreateIndex(
                name: "IX_MonAn_danh_muc_id",
                table: "MonAn",
                column: "danh_muc_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "MonAn");

            migrationBuilder.DropTable(
                name: "BanAn");

            migrationBuilder.DropTable(
                name: "DanhMuc");
        }
    }
}
