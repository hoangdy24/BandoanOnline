using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class TaoNhanVienViewModel
    {
        public string NhanVienId { get; set; }
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [Display(Name = "Mã nhân viên")]
        [StringLength(20, ErrorMessage = "Mã nhân viên không vượt quá 20 ký tự")]
        public string MaNhanVien { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "CCCD là bắt buộc")]
        [Display(Name = "Số CCCD")]
        [StringLength(12, MinimumLength = 9, ErrorMessage = "CCCD phải từ 9-12 số")]
        public string CCCD { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        [Display(Name = "Chức vụ")]
        public string ChucVu { get; set; }

        [Required(ErrorMessage = "Cửa hàng là bắt buộc")]
        [Display(Name = "Cửa hàng")]
        public int CuaHangId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu làm là bắt buộc")]
        [Display(Name = "Ngày bắt đầu làm")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDauLam { get; set; } = DateTime.Now;

        [Display(Name = "Đang làm việc")]
        public bool DangLamViec { get; set; } = true;

        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Vai trò")]
        public string Role { get; set; }

        public SelectList CuaHangOptions { get; set; }
        public SelectList RoleOptions { get; set; }
    }
}