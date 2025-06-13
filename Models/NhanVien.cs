using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Models
{
    public class NhanVien
    {
        [Key]
        public string NhanVienId { get; set; }

        public string MaNhanVien { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string CCCD { get; set; }
        public string ChucVu { get; set; }
        public int CuaHangId { get; set; }
        public DateTime NgayBatDauLam { get; set; }
        public bool DangLamViec { get; set; }

        [ForeignKey("NhanVienId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("CuaHangId")]
        public virtual CuaHang CuaHang { get; set; }

        public virtual ICollection<DonHang> DonHang { get; set; }
        public virtual ICollection<DonHang> DonHangTiepNhan { get; set; }
        public virtual ICollection<DonHang> DonHangGiaoHang { get; set; }
    }
}