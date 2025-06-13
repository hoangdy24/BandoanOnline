using System;
using System.Collections.Generic;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Areas.Admin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }

        // Percentage changes
        public decimal OrderChangePercent { get; set; }
        public decimal RevenueChangePercent { get; set; }
        public decimal CustomerChangePercent { get; set; }
        public decimal ProductChangePercent { get; set; }

        // Recent orders
        public List<DonHang> RecentOrders { get; set; } = new List<DonHang>();

        // Recent activities
        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();

        // Recent reviews
        public List<DanhGiaDonHang> RecentReviews { get; set; } = new List<DanhGiaDonHang>();

        // Business goals progress
        public decimal RevenueGoalProgress { get; set; }
        public decimal NewCustomerGoalProgress { get; set; }
        public decimal OrderGoalProgress { get; set; }
        public decimal PositiveReviewGoalProgress { get; set; }

        // Revenue chart data
        public List<decimal> MonthlyRevenue { get; set; } = new List<decimal>();
    }
    public class ActivityItem
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string IconColor { get; set; }
        public DateTime Time { get; set; }
        public string TimeAgo => GetTimeAgo(Time);

        private string GetTimeAgo(DateTime dateTime)
        {
            if (dateTime > DateTime.Now)
            {
                return "vừa xong";
            }

            TimeSpan timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return $"{(int)timeSpan.TotalSeconds} giây trước";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";

            return $"{(int)timeSpan.TotalDays} ngày trước";
        }
    }
}
