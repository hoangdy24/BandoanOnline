using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace QlW_BandoanOnline.Models
{
    public class GioHangItem
    {
        [Key]
        public int MaGioHangItem { get; set; }

        public int MaGioHang { get; set; }
        public int MaSanPham { get; set; }

        [Required]
        [StringLength(100)]
        public string TenSanPham { get; set; }

        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public string TuyChon { get; set; }
        public string GhiChu { get; set; }

        [ForeignKey("MaGioHang")]
        public virtual GioHang GioHang { get; set; }

        [ForeignKey("MaSanPham")]
        public virtual SanPham SanPham { get; set; }

        [NotMapped]
        public decimal ThanhTien => (GiaBan + TinhGiaTuyChon()) * SoLuong;

        private decimal TinhGiaTuyChon()
        {
            if (string.IsNullOrEmpty(TuyChon))
                return 0;

            try
            {
                var tuyChons = JsonConvert.DeserializeObject<List<TuyChonGioHang>>(TuyChon);
                return tuyChons?.Sum(t => t.GiaThem) ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}