using QlW_BandoanOnline.Models;
using X.PagedList;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class ThongKeDSUserViewModel
    {
        public IPagedList<ApplicationUser> Users { get; set; }
        public IPagedList<KhachHang> Customers { get; set; }
        public ThongKeTaiKhoanViewModel Statistics
        {
            get; set;
        }
    }
}
