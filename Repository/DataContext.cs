using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QlW_BandoanOnline.Repository
{
    public class DataContext : IdentityDbContext<ApplicationUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<CuaHang> CuaHang { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<SanPham> SanPham { get; set; }
        public DbSet<TuyChonSanPham> TuyChonSanPham { get; set; }
        public DbSet<ChiTietTuyChon> ChiTietTuyChon { get; set; }
        public DbSet<KhuyenMai> KhuyenMai { get; set; }
        public DbSet<GioHang> GioHang { get; set; }
        public DbSet<GioHangItem> GioHangItem { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
        public DbSet<DanhGiaDonHang> DanhGiaDonHang { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<DonHangHistory> DonHangHistory { get; set; }
        public DbSet<MucTieuDoanhThu> MucTieuDoanhThu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Identity và các quan hệ chính
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.NhanVien)
                .WithOne(n => n.User)
                .HasForeignKey<NhanVien>(n => n.NhanVienId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Cấu hình KhachHang
            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.HasKey(k => k.UserId);
                entity.HasOne(k => k.User)
                    .WithOne()
                    .HasForeignKey<KhachHang>(k => k.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Cấu hình quan hệ DonHang
            modelBuilder.Entity<DonHang>(entity =>
            {
                // Quan hệ với User/KhachHang
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với Nhân viên tiếp nhận
                entity.HasOne(d => d.NhanVienTiepNhan)
                    .WithMany(n => n.DonHangTiepNhan)
                    .HasForeignKey(d => d.NhanVienTiepNhanId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với Nhân viên giao hàng
                entity.HasOne(d => d.NhanVienGiaoHang)
                    .WithMany(n => n.DonHangGiaoHang)
                    .HasForeignKey(d => d.NhanVienGiaoHangId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ với Giỏ hàng
                entity.HasOne(d => d.GioHang)
                    .WithMany()
                    .HasForeignKey(d => d.MaGioHang)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ với Chi tiết đơn hàng
                entity.HasMany(d => d.ChiTietDonHang)
                    .WithOne(c => c.DonHang)
                    .HasForeignKey(c => c.MaDonHang);

                // Cấu hình enum
                entity.Property(d => d.PhuongThucThanhToan).HasConversion<string>();
                entity.Property(d => d.TrangThai).HasConversion<string>();

                // Index
                entity.HasIndex(d => d.MaDonHangPublic).IsUnique();
                entity.HasIndex(d => d.ThoiGianDatHang);

                entity.HasIndex(d => new { d.UserId, d.MaKhuyenMai })
                      .IsUnique(false)
                      .HasFilter("[MaKhuyenMai] IS NOT NULL");
            });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.SanPham)
                .WithMany()
                .HasForeignKey(ct => ct.MaSanPham)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // 4. Cập nhật model NhanVien để hỗ trợ quan hệ mới
            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.HasMany(n => n.DonHangTiepNhan)
                    .WithOne(d => d.NhanVienTiepNhan)
                    .HasForeignKey(d => d.NhanVienTiepNhanId);

                entity.HasMany(n => n.DonHangGiaoHang)
                    .WithOne(d => d.NhanVienGiaoHang)
                    .HasForeignKey(d => d.NhanVienGiaoHangId);
            });

            // 5. Cấu hình Giỏ hàng
            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasMany(g => g.GioHangItem)
                    .WithOne(i => i.GioHang)
                    .HasForeignKey(i => i.MaGioHang);

                entity.HasOne(g => g.User)
                    .WithMany()
                    .HasForeignKey(g => g.UserId);
            });

            // 7. Cấu hình mặc định cho ngày tạo
            modelBuilder.Entity<SanPham>()
                .Property(s => s.NgayTao)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<GioHang>()
                .Property(g => g.NgayTao)
                .HasDefaultValueSql("GETDATE()");

            // 8. Cấu hình DanhGiaDonHang
            modelBuilder.Entity<DanhGiaDonHang>()
                .HasOne(d => d.DonHang)
                .WithMany()
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}