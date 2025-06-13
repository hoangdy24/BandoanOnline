using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using QlW_BandoanOnline.Controllers;

namespace QlW_BandoanOnline.Models
{
    public class DanhGiaDonHang
    {
        [Key]
        public int MaDanhGia { get; set; }

        public int MaDonHang { get; set; }
        public string UserId { get; set; }
        public int Diem { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; }
        public string PhanHoi { get; set; }
        public DateTime? NgayPhanHoi { get; set; }

        [ForeignKey("MaDonHang")]
        public virtual DonHang DonHang { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}