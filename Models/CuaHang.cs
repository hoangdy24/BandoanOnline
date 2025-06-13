using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QlW_BandoanOnline.Models;
namespace QlW_BandoanOnline.Models
{
    public class CuaHang
    {
        [Key]
        public int CuaHangId { get; set; }
        public string TenCuaHang { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public TimeSpan GioMoCua { get; set; }
        public TimeSpan GioDongCua { get; set; }

        public virtual ICollection<NhanVien> NhanVien { get; set; }
        public virtual ICollection<DanhMuc> DanhMuc { get; set; }
        [NotMapped]
        public bool DangMoCua
        {
            get
            {
                var now = DateTime.Now.TimeOfDay;
                return now >= GioMoCua && now <= GioDongCua;
            }
        }
    }
}