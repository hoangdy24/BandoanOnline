using System.ComponentModel.DataAnnotations;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class ThongKeTaiKhoanViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
    }
}