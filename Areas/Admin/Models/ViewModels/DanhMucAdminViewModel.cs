using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QlW_BandoanOnline.Repository.Validation;

namespace QlW_BandoanOnline.Models.ViewModels
{
    public class DanhMucAdminViewModel
    {
        public int MaDanhMuc { get; set; }
        
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự")]
        public string TenDanhMuc { get; set; }
        
        [StringLength(200, ErrorMessage = "Mô tả không quá 200 ký tự")]
        public string MoTa { get; set; }
        
        [FileExtension(ErrorMessage = "Chỉ chấp nhận file ảnh có đuôi .jpg, .jpeg, .png hoặc .webp")]
        [DataType(DataType.Upload)]
        public IFormFile HinhAnhFile { get; set; }
        
        public string HinhAnh { get; set; }
        
        public int ThuTuHienThi { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? CuaHangId { get; set; }
    }
}