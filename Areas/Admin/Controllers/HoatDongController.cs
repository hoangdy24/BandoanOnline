using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using QlW_BandoanOnline.Repository;


namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class HoatDongController : Controller
    {
        private readonly DataContext _context;

        public HoatDongController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var query = _context.DonHangHistory
                .Include(h => h.DonHang)
                .Include(h => h.NhanVien)
                .OrderByDescending(h => h.ThoiGianCapNhat);

            var totalItems = await query.CountAsync();
            var activities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new ActivityListViewModel
            {
                Activities = activities.Select(a => new ActivityItem
                {
                    Title = $"Đơn hàng #{a.DonHang.MaDonHangPublic} đã chuyển sang trạng thái {GetStatusName(a.TrangThai)}",
                    Icon = GetStatusIcon(a.TrangThai),
                    IconColor = GetStatusColor(a.TrangThai),
                    Time = a.ThoiGianCapNhat
                }).ToList(),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                }
            };

            return View(model);
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
