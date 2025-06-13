using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using QlW_BandoanOnline.Models;

public class ApplicationUser : IdentityUser
{
    public string HoTen { get; set; }
    public DateTime NgaySinh { get; set; }
    public string DiaChi { get; set; }
    public virtual NhanVien NhanVien { get; set; }
    public virtual ICollection<DonHang> DonHang { get; set; }
    public virtual ICollection<GioHang> GioHang { get; set; }
}