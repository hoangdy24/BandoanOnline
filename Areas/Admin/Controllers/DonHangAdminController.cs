using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class DonHangAdminController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonHangAdminController(DataContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();

            return View(donHangs);
        }

        // Đơn hàng đang xử lý
        public async Task<IActionResult> DangXuLy()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == user.Id &&
                          (d.TrangThai == TrangThaiDonHang.ChoXacNhan ||
                           d.TrangThai == TrangThaiDonHang.DaXacNhan ||
                           d.TrangThai == TrangThaiDonHang.DangChuanBi))
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();

            return View("Index", donHangs);
        }

        // Đơn hàng đang giao
        public async Task<IActionResult> DangGiao()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == user.Id &&
                          d.TrangThai == TrangThaiDonHang.DangGiaoHang)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();

            return View("Index", donHangs);
        }

        // Đơn hàng hoàn thành
        public async Task<IActionResult> HoanThanh()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == user.Id &&
                          d.TrangThai == TrangThaiDonHang.DaGiaoThanhCong)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();

            return View("Index", donHangs);
        }

        // Đơn hàng đã hủy
        public async Task<IActionResult> DaHuy()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == user.Id &&
                          d.TrangThai == TrangThaiDonHang.DaHuy)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();

            return View("Index", donHangs);
        }

        // Thanh toán VNPAY
        public async Task<IActionResult> VNPAY()
        {
            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.PhuongThucThanhToan == PhuongThucThanhToan.VNPay)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();
            return View("Index", donHangs);
        }

        // Thanh toán tiền mặt
        public async Task<IActionResult> TienMat()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHangs = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == user.Id &&
                          d.PhuongThucThanhToan == PhuongThucThanhToan.TienMat)
                .OrderByDescending(d => d.ThoiGianDatHang)
                .ToListAsync();
            return View("Index", donHangs);
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> ChiTiet(int id)
        {
            var donHang = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .ThenInclude(ct => ct.SanPham)
                .Include(d => d.KhuyenMai)
                .Include(d => d.NhanVienGiaoHang)
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            // Lấy lịch sử đơn hàng
            var lichSu = await _context.DonHangHistory
                .Include(h => h.NhanVien)
                .Where(h => h.MaDonHang == id)
                .OrderBy(h => h.ThoiGianCapNhat)
                .ToListAsync();

            ViewBag.LichSuDonHang = lichSu;

            return View(donHang);
        }

        // Hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home");

            var donHang = await _context.DonHang
                .FirstOrDefaultAsync(d => d.MaDonHang == id && d.UserId == user.Id);

            if (donHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index");
            }

            if (!donHang.CoTheHuy)
            {
                TempData["ErrorMessage"] = "Đơn hàng không thể hủy ở trạng thái hiện tại";
                return RedirectToAction("ChiTiet", new { id });
            }

            donHang.TrangThai = TrangThaiDonHang.DaHuy;
            donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

            // Lưu lịch sử
            var history = new DonHangHistory
            {
                MaDonHang = id,
                TrangThai = TrangThaiDonHang.DaHuy,
                ThoiGianCapNhat = DateTime.Now,
                GhiChu = "Khách hàng hủy đơn hàng"
            };

            _context.DonHangHistory.Add(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công";
            return RedirectToAction("ChiTiet", new { id });
        }
    }
}
