using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Controllers
{
    public class ThucdonController : Controller
    {
        private readonly DataContext _context;
        public ThucdonController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? MaDanhMuc, int page = 1)
        {

            int pageSize = 12; // Số sản phẩm mỗi trang
            var sanPhams = _context.SanPham.AsQueryable();

            if (MaDanhMuc != null)
            {
                sanPhams = sanPhams.Where(sp => sp.MaDanhMuc == MaDanhMuc);
                ViewBag.CurrentCategory = MaDanhMuc;
            }

            IQueryable<SanPham> sanPham = _context.SanPham
                .Where(sp => sp.IsActive)
                .Include(sp => sp.DanhMuc);

            // Lọc theo danh mục nếu có
            if (MaDanhMuc.HasValue)
            {
                sanPhams = sanPhams.Where(sp => sp.MaDanhMuc == MaDanhMuc);
            }

            // Lấy danh sách danh mục để hiển thị menu
            ViewBag.DanhMuc = _context.DanhMuc
                .Where(dm => dm.IsActive)
                .OrderBy(dm => dm.ThuTuHienThi)
                .ToList();

            // Truyền MaDanhMuc hiện tại để highlight
            ViewBag.CurrentCategory = MaDanhMuc;

            var count = sanPhams.Count();
            var items = sanPhams.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(items);
        }
        public IActionResult ChiTietSanPham(int id)
        {
            var sanPham = _context.SanPham.FirstOrDefault(sp => sp.MaSanPham == id);
            if (sanPham == null) return NotFound();

            var tuyChons = _context.TuyChonSanPham
                .Where(tc => tc.MaSanPham == id)
                .ToList();

            var chiTietDict = tuyChons.ToDictionary(
                tc => tc.MaTuyChon,
                tc => _context.ChiTietTuyChon
                    .Where(ct => ct.MaTuyChon == tc.MaTuyChon)
                    .ToList()
            );

            var viewModel = new ChiTietSanPhamViewModel
            {
                SanPham = sanPham,
                TuyChons = tuyChons,
                ChiTietTheoTuyChon = chiTietDict
            };

            return View(viewModel);
        }
    }
}
