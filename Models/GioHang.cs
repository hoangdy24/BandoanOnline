using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QlW_BandoanOnline.Models
{
    public class GioHang
    {
        [Key]
        public int MaGioHang { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<GioHangItem> GioHangItem { get; set; }

        [NotMapped]
        public decimal TongTien => GioHangItem?.Sum(i => i.ThanhTien) ?? 0;

        [NotMapped]
        public int TongSoLuong => GioHangItem?.Sum(i => i.SoLuong) ?? 0;
    }
}