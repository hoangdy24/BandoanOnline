using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class DanhGiaDonHangController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DanhGiaDonHangController(
            DataContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị form đánh giá đơn hàng
        [HttpGet]
        public async Task<IActionResult> Create(int maDonHang)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra đơn hàng có thuộc về người dùng này và đã giao thành công chưa
            var donHang = await _context.DonHang
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang &&
                                         dh.UserId == user.Id &&
                                         dh.TrangThai == TrangThaiDonHang.DaGiaoThanhCong);

            if (donHang == null)
            {
                return NotFound("Đơn hàng không tồn tại hoặc không thể đánh giá");
            }

            // Kiểm tra xem đã đánh giá chưa
            var daDanhGia = await _context.DanhGiaDonHang
                .AnyAsync(dg => dg.MaDonHang == maDonHang && dg.UserId == user.Id);

            if (daDanhGia)
            {
                return RedirectToAction("ChiTiet", "TheoDoiDonHang", new { maDonHang = maDonHang });
            }

            var model = new DanhGiaDonHang
            {
                MaDonHang = maDonHang,
                UserId = user.Id,
                NgayDanhGia = DateTime.Now
            };

            return View(model);
        }

        // Xử lý submit đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaDonHang model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra lại điều kiện đơn hàng
            var donHang = await _context.DonHang
                .FirstOrDefaultAsync(dh => dh.MaDonHang == model.MaDonHang &&
                                         dh.UserId == user.Id &&
                                         dh.TrangThai == TrangThaiDonHang.DaGiaoThanhCong);

            if (donHang == null)
            {
                return NotFound("Đơn hàng không tồn tại hoặc không thể đánh giá");
            }

            // Kiểm tra xem đã đánh giá chưa
            var daDanhGia = await _context.DanhGiaDonHang
                .AnyAsync(dg => dg.MaDonHang == model.MaDonHang && dg.UserId == user.Id);

            if (daDanhGia)
            {
                return RedirectToAction("ChiTiet", "TheoDoiDonHang", new { maDonHang = model.MaDonHang });
            }

            // Tạo đánh giá mới
            var danhGia = new DanhGiaDonHang
            {
                MaDonHang = model.MaDonHang,
                UserId = user.Id,
                Diem = model.Diem,
                NoiDung = model.NoiDung,
                NgayDanhGia = DateTime.Now
            };

            _context.DanhGiaDonHang.Add(danhGia);
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Cảm ơn bạn đã đánh giá đơn hàng!";
            TempData["ToastType"] = "success";
            return RedirectToAction("ChiTiet", "TheoDoiDonHang", new { id = model.MaDonHang });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int maDonHang)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var danhGia = await _context.DanhGiaDonHang
                .Include(dg => dg.DonHang)
                .Include(dg => dg.User)
                .FirstOrDefaultAsync(dg => dg.MaDonHang == maDonHang &&
                                        (dg.UserId == user.Id || User.IsInRole("Admin") || User.IsInRole("NhanVien")));

            if (danhGia == null)
            {
                return NotFound("Không tìm thấy đánh giá");
            }

            return View(danhGia);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanHoi(int maDanhGia, string phanHoi)
        {
            var danhGia = await _context.DanhGiaDonHang.FindAsync(maDanhGia);
            if (danhGia == null)
            {
                return NotFound();
            }

            danhGia.PhanHoi = phanHoi;
            danhGia.NgayPhanHoi = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = "Đã gửi phản hồi thành công";
            TempData["ToastType"] = "success";
            return RedirectToAction("Details", new { maDonHang = danhGia.MaDonHang });
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index()
        {
            var danhSachDanhGia = await _context.DanhGiaDonHang
                .Include(dg => dg.DonHang)
                .Include(dg => dg.User)
                .OrderByDescending(dg => dg.NgayDanhGia)
                .ToListAsync();

            return View(danhSachDanhGia);
        }
    }
}
