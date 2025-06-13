using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QlW_BandoanOnline.Controllers;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Models
{
    public class KhuyenMai
    {
        [Key]
        public int MaKhuyenMai { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKhuyenMai { get; set; }

        public string MaKhuyenMaiCode { get; set; } // VD: "SUMMER2024", "GIAM50K"
        public string? HinhAnh { get; set; }

        public string MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaTriGiam { get; set; } // Phần trăm (10.5%) hoặc số tiền (50000)

        public bool LaPhanTram { get; set; } // True: giảm %, False: giảm tiền trực tiếp

        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }

        public int SoLuong { get; set; } // Số lượng mã khuyến mãi áp dụng
        public int DaSuDung { get; set; } = 0; // Đếm số lần đã sử dụng

        public bool IsActive { get; set; } = true;

        // Quan hệ với sản phẩm/danh mục (nếu áp dụng cho sản phẩm cụ thể)
        public int? MaSanPham { get; set; }
        public int? MaDanhMuc { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham? SanPham { get; set; }

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        // Quan hệ với đơn hàng
        public virtual ICollection<DonHang> DonHang { get; set; }
    }
}