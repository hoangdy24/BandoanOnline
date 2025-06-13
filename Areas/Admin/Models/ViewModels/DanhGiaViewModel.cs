using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class DanhGiaViewModel
    {
        public List<DanhGiaDonHang> Reviews { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int? FilterRating { get; set; }
    }
}
