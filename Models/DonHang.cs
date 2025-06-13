using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace QlW_BandoanOnline.Models
{
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        // Thông tin đơn hàng
        public string MaDonHangPublic { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        public string UserId { get; set; }
        public string HoTenKhach { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string GhiChuDiaChi { get; set; }
        public DateTime ThoiGianDatHang { get; set; } = DateTime.Now;
        public DateTime? ThoiGianGiaoDuKien { get; set; }

        // Thanh toán
        public PhuongThucThanhToan PhuongThucThanhToan { get; set; }
        public bool DaThanhToan { get; set; } = false;
        public DateTime? ThoiGianThanhToan { get; set; }

        // Quản lý nhân viên
        public string NhanVienTiepNhanId { get; set; }
        public string NhanVienGiaoHangId { get; set; }

        public string TenNhanVienTiepNhan => NhanVienTiepNhan?.HoTen ?? "Chưa có";
        public string TenNhanVienGiaoHang => NhanVienGiaoHang?.HoTen ?? "Chưa có";

        // Giỏ hàng
        public int? MaGioHang { get; set; }

        // Trạng thái đơn
        public TrangThaiDonHang TrangThai { get; set; } = TrangThaiDonHang.ChoXacNhan;
        public DateTime? ThoiGianCapNhatTrangThai { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("NhanVienTiepNhanId")]
        public virtual NhanVien NhanVienTiepNhan { get; set; }

        [ForeignKey("NhanVienGiaoHangId")]
        public virtual NhanVien NhanVienGiaoHang { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang GioHang { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; } = new List<ChiTietDonHang>();

        public virtual VnPayTransaction VnPayTransaction { get; set; }

        public int? MaKhuyenMai { get; set; }
        public decimal GiamGia { get; set; } = 0;
        public decimal TienGiaoHang { get; set; } = 20000;

        [ForeignKey("MaKhuyenMai")]
        public virtual KhuyenMai KhuyenMai { get; set; }

        // Tổng tiền tính toán (tạm thời)
        [NotMapped]
        public decimal TongTienTruocGiamGia => ChiTietDonHang?.Sum(ct => ct.ThanhTien) ?? 0;

        [NotMapped]
        public decimal TongTien => TongTienTruocGiamGia - GiamGia + TienGiaoHang;

        // Tổng tiền lưu vào database
        public decimal TongTienTruocGiamGiaDB { get; set; }
        public decimal TongTienSauGiamGiaDB { get; set; }

        [NotMapped]
        public bool CoTheHuy
        {
            get
            {
                return TrangThai != TrangThaiDonHang.DaHuy &&
                       TrangThai != TrangThaiDonHang.DaGiaoThanhCong;
            }
        }

        // Hàm cập nhật tổng tiền
        public void CapNhatTongTien()
        {
            TongTienTruocGiamGiaDB = ChiTietDonHang?.Sum(ct => ct.ThanhTien) ?? 0;
            TongTienSauGiamGiaDB = TongTienTruocGiamGiaDB - GiamGia + TienGiaoHang;
        }
    }

    public enum PhuongThucThanhToan
    {
        [Display(Name = "Tiền mặt")]
        TienMat,

        [Display(Name = "VNPay")]
        VNPay,
    }

    public enum TrangThaiDonHang
    {
        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan,

        [Display(Name = "Đã xác nhận")]
        DaXacNhan,

        [Display(Name = "Đang chuẩn bị")]
        DangChuanBi,

        [Display(Name = "Đang giao hàng")]
        DangGiaoHang,

        [Display(Name = "Đã giao thành công")]
        DaGiaoThanhCong,

        [Display(Name = "Đã hủy")]
        DaHuy,

        [Display(Name = "Hoàn tiền")]
        HoanTien
    }
}
