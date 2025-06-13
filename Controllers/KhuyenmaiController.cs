using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Controllers
{
    public class KhuyenmaiController : Controller
    {
        private readonly DataContext _context;

        public KhuyenmaiController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var khuyenMais = await _context.KhuyenMai
                .Where(k => k.IsActive
                          && k.NgayBatDau <= now
                          && k.NgayKetThuc >= now
                          && k.SoLuong > k.DaSuDung)
                .Include(k => k.SanPham)
                .Include(k => k.DanhMuc)
                .ToListAsync();

            return View(khuyenMais);
        }

        // GET: Chi tiết khuyến mãi
        public async Task<IActionResult> ChiTiet(int id)
        {
            var khuyenMai = await _context.KhuyenMai
                .Include(k => k.SanPham)
                .Include(k => k.DanhMuc)
                .FirstOrDefaultAsync(m => m.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            // Kiểm tra khuyến mãi còn hiệu lực
            var now = DateTime.Now;
            if (!khuyenMai.IsActive || khuyenMai.NgayBatDau > now || khuyenMai.NgayKetThuc < now || khuyenMai.SoLuong <= khuyenMai.DaSuDung)
            {
                ViewBag.HetHieuLuc = true;
            }

            var randomSanPhams = await _context.SanPham
            .Where(sp => sp.MaSanPham != khuyenMai.MaSanPham) // Loại bỏ sản phẩm đang xem (nếu có)
            .OrderBy(x => Guid.NewGuid()) // Sắp xếp ngẫu nhiên
            .Take(4) // Lấy 4 sản phẩm
            .ToListAsync();

            ViewBag.RandomSanPhams = randomSanPhams;

            return View(khuyenMai);
        }

        // API kiểm tra mã khuyến mãi (dùng cho giỏ hàng)
        [HttpGet]
        public async Task<IActionResult> KiemTraMa(string maKhuyenMai)
        {
            var now = DateTime.Now;
            var promotion = await _context.KhuyenMai
                .FirstOrDefaultAsync(k => k.MaKhuyenMaiCode == maKhuyenMai
                                       && k.IsActive
                                       && k.NgayBatDau <= now
                                       && k.NgayKetThuc >= now
                                       && k.SoLuong > k.DaSuDung);

            if (promotion == null)
            {
                return Json(new { success = false, toastMessage = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn", toastType = "error" });
            }

            return Json(new
            {
                success = true,
                id = promotion.MaKhuyenMai,
                ten = promotion.TenKhuyenMai,
                loai = promotion.LaPhanTram ? "percent" : "fixed",
                giatri = promotion.GiaTriGiam
            });
        }
    }
}
