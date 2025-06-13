using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QlW_BandoanOnline.Migrations
{
    /// <inheritdoc />
    public partial class DATA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuaHang",
                columns: table => new
                {
                    CuaHangId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCuaHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioMoCua = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioDongCua = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuaHang", x => x.CuaHangId);
                });

            migrationBuilder.CreateTable(
                name: "MucTieuDoanhThu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevenueTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewCustomerTarget = table.Column<int>(type: "int", nullable: false),
                    OrderTarget = table.Column<int>(type: "int", nullable: false),
                    PositiveReviewTarget = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MucTieuDoanhThu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHang",
                columns: table => new
                {
                    MaGioHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHang", x => x.MaGioHang);
                    table.ForeignKey(
                        name: "FK_GioHang_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GioHang_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_KhachHang_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThuTuHienThi = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CuaHangId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.MaDanhMuc);
                    table.ForeignKey(
                        name: "FK_DanhMuc_CuaHang_CuaHangId",
                        column: x => x.CuaHangId,
                        principalTable: "CuaHang",
                        principalColumn: "CuaHangId");
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    NhanVienId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNhanVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CuaHangId = table.Column<int>(type: "int", nullable: false),
                    NgayBatDauLam = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DangLamViec = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.NhanVienId);
                    table.ForeignKey(
                        name: "FK_NhanVien_AspNetUsers_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NhanVien_CuaHang_CuaHangId",
                        column: x => x.CuaHangId,
                        principalTable: "CuaHang",
                        principalColumn: "CuaHangId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    MaSanPham = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSanPham = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaGoc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianChuanBi = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsBestSeller = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.MaSanPham);
                    table.ForeignKey(
                        name: "FK_SanPham_DanhMuc_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMuc",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangItem",
                columns: table => new
                {
                    MaGioHangItem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGioHang = table.Column<int>(type: "int", nullable: false),
                    MaSanPham = table.Column<int>(type: "int", nullable: false),
                    TenSanPham = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TuyChon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangItem", x => x.MaGioHangItem);
                    table.ForeignKey(
                        name: "FK_GioHangItem_GioHang_MaGioHang",
                        column: x => x.MaGioHang,
                        principalTable: "GioHang",
                        principalColumn: "MaGioHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GioHangItem_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMai",
                columns: table => new
                {
                    MaKhuyenMai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhuyenMai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaKhuyenMaiCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaPhanTram = table.Column<bool>(type: "bit", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DaSuDung = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaSanPham = table.Column<int>(type: "int", nullable: true),
                    MaDanhMuc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMai", x => x.MaKhuyenMai);
                    table.ForeignKey(
                        name: "FK_KhuyenMai_DanhMuc_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMuc",
                        principalColumn: "MaDanhMuc");
                    table.ForeignKey(
                        name: "FK_KhuyenMai_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham");
                });

            migrationBuilder.CreateTable(
                name: "TuyChonSanPham",
                columns: table => new
                {
                    MaTuyChon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTuyChon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BatBuoc = table.Column<bool>(type: "bit", nullable: false),
                    SoLuongChonToiDa = table.Column<int>(type: "int", nullable: false),
                    SoLuongChonToiThieu = table.Column<int>(type: "int", nullable: false),
                    MaSanPham = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuyChonSanPham", x => x.MaTuyChon);
                    table.ForeignKey(
                        name: "FK_TuyChonSanPham_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    MaDonHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHangPublic = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HoTenKhach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuDiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianDatHang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianGiaoDuKien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaThanhToan = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGianThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanVienTiepNhanId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NhanVienGiaoHangId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaGioHang = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianCapNhatTrangThai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaKhuyenMai = table.Column<int>(type: "int", nullable: true),
                    GiamGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TienGiaoHang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTienTruocGiamGiaDB = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTienSauGiamGiaDB = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    KhachHangUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NhanVienId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.MaDonHang);
                    table.ForeignKey(
                        name: "FK_DonHang_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DonHang_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHang_GioHang_MaGioHang",
                        column: x => x.MaGioHang,
                        principalTable: "GioHang",
                        principalColumn: "MaGioHang",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DonHang_KhachHang_KhachHangUserId",
                        column: x => x.KhachHangUserId,
                        principalTable: "KhachHang",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_DonHang_KhuyenMai_MaKhuyenMai",
                        column: x => x.MaKhuyenMai,
                        principalTable: "KhuyenMai",
                        principalColumn: "MaKhuyenMai");
                    table.ForeignKey(
                        name: "FK_DonHang_NhanVien_NhanVienGiaoHangId",
                        column: x => x.NhanVienGiaoHangId,
                        principalTable: "NhanVien",
                        principalColumn: "NhanVienId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHang_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "NhanVienId");
                    table.ForeignKey(
                        name: "FK_DonHang_NhanVien_NhanVienTiepNhanId",
                        column: x => x.NhanVienTiepNhanId,
                        principalTable: "NhanVien",
                        principalColumn: "NhanVienId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietTuyChon",
                columns: table => new
                {
                    MaChiTietTuyChon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChiTiet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaThem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaTuyChon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietTuyChon", x => x.MaChiTietTuyChon);
                    table.ForeignKey(
                        name: "FK_ChiTietTuyChon_TuyChonSanPham_MaTuyChon",
                        column: x => x.MaTuyChon,
                        principalTable: "TuyChonSanPham",
                        principalColumn: "MaTuyChon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaSanPham = table.Column<int>(type: "int", nullable: true),
                    TenSanPham = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TuyChonThem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SanPhamMaSanPham = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_SanPham_SanPhamMaSanPham",
                        column: x => x.SanPhamMaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham");
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaDonHang",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Diem = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhanHoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KhachHangUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaDonHang", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGiaDonHang_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhGiaDonHang_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang");
                    table.ForeignKey(
                        name: "FK_DanhGiaDonHang_KhachHang_KhachHangUserId",
                        column: x => x.KhachHangUserId,
                        principalTable: "KhachHang",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DonHangHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    ThoiGianCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhanVienId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonHangHistory_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonHangHistory_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "NhanVienId");
                });

            migrationBuilder.CreateTable(
                name: "VnPayTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<int>(type: "int", nullable: false),
                    MaGiaoDich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TxnRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecureHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VnPayTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VnPayTransaction_DonHang_MaDonHang",
                        column: x => x.MaDonHang,
                        principalTable: "DonHang",
                        principalColumn: "MaDonHang",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaDonHang",
                table: "ChiTietDonHang",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MaSanPham",
                table: "ChiTietDonHang",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_SanPhamMaSanPham",
                table: "ChiTietDonHang",
                column: "SanPhamMaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietTuyChon_MaTuyChon",
                table: "ChiTietTuyChon",
                column: "MaTuyChon");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaDonHang_KhachHangUserId",
                table: "DanhGiaDonHang",
                column: "KhachHangUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaDonHang_MaDonHang",
                table: "DanhGiaDonHang",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaDonHang_UserId",
                table: "DanhGiaDonHang",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_CuaHangId",
                table: "DanhMuc",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ApplicationUserId",
                table: "DonHang",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_KhachHangUserId",
                table: "DonHang",
                column: "KhachHangUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaDonHangPublic",
                table: "DonHang",
                column: "MaDonHangPublic",
                unique: true,
                filter: "[MaDonHangPublic] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaGioHang",
                table: "DonHang",
                column: "MaGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MaKhuyenMai",
                table: "DonHang",
                column: "MaKhuyenMai");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_NhanVienGiaoHangId",
                table: "DonHang",
                column: "NhanVienGiaoHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_NhanVienId",
                table: "DonHang",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_NhanVienTiepNhanId",
                table: "DonHang",
                column: "NhanVienTiepNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_ThoiGianDatHang",
                table: "DonHang",
                column: "ThoiGianDatHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_UserId_MaKhuyenMai",
                table: "DonHang",
                columns: new[] { "UserId", "MaKhuyenMai" },
                filter: "[MaKhuyenMai] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangHistory_MaDonHang",
                table: "DonHangHistory",
                column: "MaDonHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangHistory_NhanVienId",
                table: "DonHangHistory",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHang_ApplicationUserId",
                table: "GioHang",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHang_UserId",
                table: "GioHang",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangItem_MaGioHang",
                table: "GioHangItem",
                column: "MaGioHang");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangItem_MaSanPham",
                table: "GioHangItem",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMai_MaDanhMuc",
                table: "KhuyenMai",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMai_MaSanPham",
                table: "KhuyenMai",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_CuaHangId",
                table: "NhanVien",
                column: "CuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaDanhMuc",
                table: "SanPham",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_TuyChonSanPham_MaSanPham",
                table: "TuyChonSanPham",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_VnPayTransaction_MaDonHang",
                table: "VnPayTransaction",
                column: "MaDonHang",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "ChiTietTuyChon");

            migrationBuilder.DropTable(
                name: "DanhGiaDonHang");

            migrationBuilder.DropTable(
                name: "DonHangHistory");

            migrationBuilder.DropTable(
                name: "GioHangItem");

            migrationBuilder.DropTable(
                name: "MucTieuDoanhThu");

            migrationBuilder.DropTable(
                name: "VnPayTransaction");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TuyChonSanPham");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "GioHang");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "KhuyenMai");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropTable(
                name: "CuaHang");
        }
    }
}
