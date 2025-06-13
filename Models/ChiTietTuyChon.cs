using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QlW_BandoanOnline.Models
{
    public class ChiTietTuyChon
    {
        [Key]
        public int MaChiTietTuyChon { get; set; }

        [Required]
        [StringLength(100)]
        public string TenChiTiet { get; set; }

        public decimal GiaThem { get; set; }
        public int MaTuyChon { get; set; }

        [ForeignKey("MaTuyChon")]
        public virtual TuyChonSanPham TuyChonSanPham { get; set; }
    }
}