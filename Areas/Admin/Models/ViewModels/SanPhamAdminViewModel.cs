using System;
using System.ComponentModel.DataAnnotations;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class SanPhamAdminViewModel
    {
        public IEnumerable<SanPham> SanPhams { get; set; }
        public IEnumerable<TuyChonSanPham> TuyChonSanPhams { get; set; }
        public IEnumerable<ChiTietTuyChon> ChiTietTuyChons { get; set; }
        public SanPham NewSanPham { get; set; }
        public TuyChonSanPham NewTuyChon { get; set; }
        public ChiTietTuyChon NewChiTiet { get; set; }
    }
}