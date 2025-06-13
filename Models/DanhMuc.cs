using QlW_BandoanOnline.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DanhMuc
{
    [Key]
    public int MaDanhMuc { get; set; }

    [Required]
    [StringLength(100)]
    public string TenDanhMuc { get; set; }

    [StringLength(200)]
    public string MoTa { get; set; }
    public string HinhAnh { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CuaHangId { get; set; }

    [ForeignKey("CuaHangId")]
    public virtual CuaHang CuaHang { get; set; }

    public virtual ICollection<SanPham> SanPham { get; set; }
}