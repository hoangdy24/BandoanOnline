using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using System.Linq.Expressions;
using System.Globalization;

namespace QlW_BandoanOnline_Solution.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(DataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy các thông tin tổng quát
            var totalOrders = await _context.DonHang.CountAsync();
            var totalRevenue = await _context.DonHang
                .Where(dh => dh.TrangThai == TrangThaiDonHang.DaGiaoThanhCong)
                .SumAsync(dh => dh.TongTienSauGiamGiaDB);
            var totalCustomers = await _context.KhachHang.CountAsync();
            var totalProducts = await _context.SanPham.CountAsync();

            // Khởi tạo ViewModel
            var model = new DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,

                OrderChangePercent = 12.5m,
                RevenueChangePercent = 8.2m,
                CustomerChangePercent = 5.3m,
                ProductChangePercent = -2.1m,

                RecentOrders = await _context.DonHang
                    .Include(dh => dh.User)
                    .OrderByDescending(dh => dh.ThoiGianDatHang)
                    .Take(5)
                    .ToListAsync(),

                RecentReviews = await _context.DanhGiaDonHang
                    .Include(dg => dg.User)
                    .Include(dg => dg.DonHang)
                    .OrderByDescending(dg => dg.NgayDanhGia)
                    .Take(3)
                    .ToListAsync(),

                MonthlyRevenue = await GetMonthlyRevenueData()
            };

            // 🔥 TÍNH TOÁN MỤC TIÊU (GOAL PROGRESS) từ DB
            var goals = await _context.MucTieuDoanhThu.FirstOrDefaultAsync();
            if (goals != null)
            {
                model.RevenueGoalProgress = goals.RevenueTarget > 0
                    ? (decimal)(model.TotalRevenue / goals.RevenueTarget * 100)
                    : 0;

                model.NewCustomerGoalProgress = goals.NewCustomerTarget > 0
                    ? (decimal)(model.TotalCustomers / (double)goals.NewCustomerTarget * 100)
                    : 0;

                model.OrderGoalProgress = goals.OrderTarget > 0
                    ? (decimal)(model.TotalOrders / (double)goals.OrderTarget * 100)
                    : 0;

                var totalReviews = await _context.DanhGiaDonHang.CountAsync();
                var positiveReviews = await _context.DanhGiaDonHang.CountAsync(r => r.Diem >= 4);
                model.PositiveReviewGoalProgress = totalReviews > 0
                    ? (decimal)(positiveReviews / (double)totalReviews * 100)
                    : 0;
            }
            else
            {
                // Nếu chưa có mục tiêu => mặc định tạm
                model.RevenueGoalProgress = 70m;
                model.NewCustomerGoalProgress = 85m;
                model.OrderGoalProgress = 65m;
                model.PositiveReviewGoalProgress = 92m;
            }

            // 🔄 Hoạt động gần đây
            model.RecentActivities = await GenerateRecentActivities();

            return View(model);
        }


        private async Task<List<decimal>> GetMonthlyRevenueData()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyRevenue = new List<decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var revenue = await _context.DonHang
                    .Where(dh => dh.ThoiGianDatHang.Year == currentYear &&
                                 dh.ThoiGianDatHang.Month == month &&
                                 dh.TrangThai == TrangThaiDonHang.DaGiaoThanhCong)
                    .SumAsync(dh => (decimal?)dh.TongTienSauGiamGiaDB) ?? 0;

                monthlyRevenue.Add(revenue / 1000000); // Convert to millions
            }

            return monthlyRevenue;
        }

        private async Task<List<ActivityItem>> GenerateRecentActivities()
        {
            var activities = new List<ActivityItem>();

            // Get recent order status changes
            var recentOrderChanges = await _context.DonHangHistory
                .Include(h => h.DonHang)
                .Include(h => h.NhanVien)
                .OrderByDescending(h => h.ThoiGianCapNhat)
                .Take(5)
                .ToListAsync();

            foreach (var change in recentOrderChanges)
            {
                activities.Add(new ActivityItem
                {
                    Title = $"Đơn hàng #{change.DonHang.MaDonHangPublic} đã chuyển sang trạng thái {GetStatusName(change.TrangThai)}",
                    Icon = GetStatusIcon(change.TrangThai),
                    IconColor = GetStatusColor(change.TrangThai),
                    Time = change.ThoiGianCapNhat
                });
            }

            // If we have less than 5 activities, fill with some default ones
            while (activities.Count < 5)
            {
                activities.Add(new ActivityItem
                {
                    Title = "Hệ thống đang hoạt động bình thường",
                    Icon = "fa-check-circle",
                    IconColor = "var(--success-color)",
                    Time = DateTime.Now.AddMinutes(-activities.Count * 30)
                });
            }

            return activities;
        }

        private string GetStatusName(TrangThaiDonHang status)
        {
            return status switch
            {
                TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
                TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
                TrangThaiDonHang.DangChuanBi => "Đang chuẩn bị",
                TrangThaiDonHang.DangGiaoHang => "Đang giao hàng",
                TrangThaiDonHang.DaGiaoThanhCong => "Đã giao thành công",
                TrangThaiDonHang.DaHuy => "Đã hủy",
                TrangThaiDonHang.HoanTien => "Hoàn tiền",
                _ => "Không xác định"
            };
        }

        private string GetStatusIcon(TrangThaiDonHang status)
        {
            return status switch
            {
                TrangThaiDonHang.ChoXacNhan => "fa-clock",
                TrangThaiDonHang.DaXacNhan => "fa-check",
                TrangThaiDonHang.DangChuanBi => "fa-utensils",
                TrangThaiDonHang.DangGiaoHang => "fa-truck",
                TrangThaiDonHang.DaGiaoThanhCong => "fa-check-circle",
                TrangThaiDonHang.DaHuy => "fa-times-circle",
                TrangThaiDonHang.HoanTien => "fa-money-bill-wave",
                _ => "fa-info-circle"
            };
        }

        private string GetStatusColor(TrangThaiDonHang status)
        {
            return status switch
            {
                TrangThaiDonHang.ChoXacNhan => "var(--warning-color)",
                TrangThaiDonHang.DaXacNhan => "var(--info-color)",
                TrangThaiDonHang.DangChuanBi => "var(--primary-color)",
                TrangThaiDonHang.DangGiaoHang => "var(--info-color)",
                TrangThaiDonHang.DaGiaoThanhCong => "var(--success-color)",
                TrangThaiDonHang.DaHuy => "var(--error-color)",
                TrangThaiDonHang.HoanTien => "var(--warning-color)",
                _ => "var(--text-color)"
            };
        }
    }
}