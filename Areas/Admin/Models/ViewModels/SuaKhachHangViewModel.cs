using System.ComponentModel.DataAnnotations;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class SuaKhachHangViewModel
    {
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string SoDienThoai { get; set; }

        [StringLength(200)]
        public string DiaChi { get; set; }

        public bool TrangThai { get; set; }
    }
}