using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Models
{
    public class KhachHang
    {
        [Key]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [StringLength(200)]
        public string DiaChi { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public bool TrangThai { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // Quan hệ với các bảng khác
        public virtual ICollection<DonHang> DonHang { get; set; }
        public virtual ICollection<DanhGiaDonHang> DanhGia { get; set; }
    }
}
