using System.ComponentModel.DataAnnotations;

namespace QlW_BandoanOnline.Models
{
    public class MucTieuDoanhThu
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Mục tiêu doanh thu (VNĐ)")]
        public decimal RevenueTarget { get; set; }

        [Display(Name = "Mục tiêu khách hàng mới")]
        public int NewCustomerTarget { get; set; }

        [Display(Name = "Mục tiêu đơn hàng")]
        public int OrderTarget { get; set; }

        [Display(Name = "Mục tiêu đánh giá tích cực (%)")]
        [Range(0, 100)]
        public int PositiveReviewTarget { get; set; }
    }
}
