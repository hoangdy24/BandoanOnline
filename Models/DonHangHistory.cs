using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QlW_BandoanOnline.Models
{
    public class DonHangHistory
    {
        [Key]
        public int Id { get; set; }

        public int MaDonHang { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang DonHang { get; set; }

        public TrangThaiDonHang TrangThai { get; set; }

        public DateTime ThoiGianCapNhat { get; set; }

        public string NhanVienId { get; set; }

        [ForeignKey("NhanVienId")]
        public virtual NhanVien NhanVien { get; set; }

        public string GhiChu { get; set; }
    }
}
