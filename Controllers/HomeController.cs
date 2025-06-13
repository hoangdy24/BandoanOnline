using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new TrangChuViewModel
            {
                // Danh sách danh mục
                DanhMucList = _context.DanhMuc
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.ThuTuHienThi)
                    .Take(3) // Lấy tối đa 8 danh mục
                    .ToList(),

                // Sản phẩm bán chạy
                SanPhamBanChay = _context.SanPham
                    .Where(sp => sp.IsActive)
                    .OrderBy(sp => sp.IsBestSeller)
                    .Take(3)
                    .ToList(),

                // Đánh giá mới nhất
                DanhGiaMoiNhat = _context.DanhGiaDonHang
                    .Include(d => d.User)
                    .Include(d => d.DonHang)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Take(3)
                    .ToList()
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode is 401 or 402 or 403 or 404)
            {
                return View("NotFound");
            }

            // For all other errors
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            });
        }
    }
}
