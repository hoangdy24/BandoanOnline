using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SanPhamAdminController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SanPhamAdminController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SanPhamAdminViewModel
            {
                SanPhams = await _context.SanPham
                        .Include(p => p.DanhMuc)
                        .Include(p => p.TuyChon)
                        .ThenInclude(t => t.ChiTietTuyChon)
                        .OrderBy(p => p.MaSanPham)
                        .ToListAsync(),

                TuyChonSanPhams = await _context.TuyChonSanPham
                        .Include(t => t.SanPham)
                        .Include(t => t.ChiTietTuyChon)
                        .ToListAsync(),

                ChiTietTuyChons = await _context.ChiTietTuyChon
                        .Include(c => c.TuyChonSanPham)
                        .ThenInclude(t => t.SanPham)
                        .ToListAsync(),

                NewSanPham = new SanPham(),
                NewTuyChon = new TuyChonSanPham(),
                NewChiTiet = new ChiTietTuyChon()
            };
            return View(viewModel);
        }

        // GET: SanPham/Create
        public IActionResult Create()
        {
            ViewBag.DanhMucList = _context.DanhMuc
                .Select(dm => new SelectListItem
                {
                    Value = dm.MaDanhMuc.ToString(),
                    Text = dm.TenDanhMuc
                }).ToList();
            return View();
        }

        // POST: SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files[0];
                        if (file != null && file.Length > 0)
                        {
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/Menu");
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            sanPham.HinhAnh = uniqueFileName;
                        }
                    }

                    sanPham.NgayTao = DateTime.Now;
                    _context.Add(sanPham);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm sản phẩm: " + ex.Message;
                }
            }

            ViewBag.DanhMucList = _context.DanhMuc
               .Select(dm => new SelectListItem
               {
                   Value = dm.MaDanhMuc.ToString(),
                   Text = dm.TenDanhMuc
               }).ToList();
            return View(sanPham);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPham.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }

            // Truyền danh sách danh mục
            ViewBag.DanhMucList = new SelectList(_context.DanhMuc, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            return View(sanPham);
        }

        // POST: SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham sanPham, IFormFile file)
        {
            if (id != sanPham.MaSanPham)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh mới nếu có
                    if (file != null && file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/Menu");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, sanPham.HinhAnh.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        sanPham.HinhAnh = uniqueFileName;
                    }

                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.MaSanPham))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message;
                }
            }

            ViewBag.DanhMucList = new SelectList(_context.DanhMuc, "MaDanhMuc", "TenDanhMuc", sanPham.MaDanhMuc);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sanPham = await _context.SanPham
                    .Include(sp => sp.TuyChon)
                        .ThenInclude(t => t.ChiTietTuyChon)
                    .Include(sp => sp.ChiTietDonHang)
                    .FirstOrDefaultAsync(sp => sp.MaSanPham == id);

                if (sanPham == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction(nameof(Index));
                }

                // 1. Xóa tất cả chi tiết tùy chọn trước
                foreach (var tuyChon in sanPham.TuyChon)
                {
                    if (tuyChon.ChiTietTuyChon != null && tuyChon.ChiTietTuyChon.Any())
                    {
                        _context.ChiTietTuyChon.RemoveRange(tuyChon.ChiTietTuyChon);
                    }
                }

                // 2. Xóa tất cả các tùy chọn sản phẩm liên quan
                if (sanPham.TuyChon != null && sanPham.TuyChon.Any())
                {
                    _context.TuyChonSanPham.RemoveRange(sanPham.TuyChon);
                }

                // 3. Cập nhật chi tiết đơn hàng
                foreach (var chiTiet in sanPham.ChiTietDonHang)
                {
                    chiTiet.MaSanPham = null;
                    chiTiet.TenSanPham = $"[Đã xóa] {chiTiet.SanPham?.TenSanPham}";
                }

                // 4. Xóa hình ảnh
                if (!string.IsNullOrEmpty(sanPham.HinhAnh))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                               "images/Menu",
                                               sanPham.HinhAnh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // 5. Xóa sản phẩm
                _context.SanPham.Remove(sanPham);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm: " + ex.Message;

                // Thêm thông tin inner exception nếu có
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += " " + ex.InnerException.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ========== TuyChonSanPham CRUD ==========

        public IActionResult CreateTuyChon(int maSanPham)
        {
            if (maSanPham == 0)
            {
                return BadRequest("maSanPham không hợp lệ hoặc không được truyền.");
            }

            var sanPham = _context.SanPham.FirstOrDefault(sp => sp.MaSanPham == maSanPham);
            if (sanPham == null)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID {maSanPham}");
            }
            ViewBag.TenSanPham = sanPham.TenSanPham;
            var model = new TuyChonSanPham
            {
                MaSanPham = maSanPham,
                SoLuongChonToiDa = 1,
                SoLuongChonToiThieu = 0
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTuyChon(TuyChonSanPham tuyChon)
        {
            if (tuyChon.MaSanPham == 0)
            {
                ModelState.AddModelError("", "Mã sản phẩm không được để 0 hoặc không hợp lệ");
            }

            if (ModelState.IsValid)
            {
                var productExists = await _context.SanPham.AnyAsync(sp => sp.MaSanPham == tuyChon.MaSanPham);
                if (!productExists)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại");
                    ViewBag.TenSanPham = "Không tìm thấy sản phẩm";
                    return View(tuyChon);
                }

                _context.Add(tuyChon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tạo tùy chọn sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            var tenSanPham = await _context.SanPham
                .Where(sp => sp.MaSanPham == tuyChon.MaSanPham)
                .Select(sp => sp.TenSanPham)
                .FirstOrDefaultAsync();

            ViewBag.TenSanPham = tenSanPham ?? "Không tìm thấy sản phẩm";
            return View(tuyChon);
        }

        public async Task<IActionResult> EditTuyChon(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuyChonSanPham = await _context.TuyChonSanPham
            .Include(t => t.SanPham)
            .FirstOrDefaultAsync(t => t.MaTuyChon == id);
            if (tuyChonSanPham == null)
            {
                return NotFound();
            }

            ViewBag.TenSanPham = tuyChonSanPham.SanPham?.TenSanPham ?? "Không xác định";
            return View(tuyChonSanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTuyChon(int id, TuyChonSanPham tuyChon)
        {
            if (id != tuyChon.MaTuyChon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tuyChon);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật tùy chọn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TuyChonExists(tuyChon.MaTuyChon))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi đồng bộ khi cập nhật tùy chọn!";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ khi cập nhật!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTuyChon(int id)
        {
            try
            {
                var tuyChon = await _context.TuyChonSanPham
                    .Include(t => t.ChiTietTuyChon)
                    .FirstOrDefaultAsync(t => t.MaTuyChon == id);

                if (tuyChon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tùy chọn cần xóa";
                    return RedirectToAction(nameof(Index));
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                if (tuyChon.ChiTietTuyChon != null && tuyChon.ChiTietTuyChon.Any())
                {
                    _context.ChiTietTuyChon.RemoveRange(tuyChon.ChiTietTuyChon);
                }

                _context.TuyChonSanPham.Remove(tuyChon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Đã xóa tùy chọn '{tuyChon.TenTuyChon}' thành công";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa tùy chọn: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TuyChonExists(int id)
        {
            return _context.TuyChonSanPham.Any(e => e.MaTuyChon == id);
        }

        // ========== ChiTietTuyChon CRUD ==========
        public IActionResult CreateChiTiet(int? optionId)
        {
            if (optionId == null)
            {
                return NotFound();
            }

            var tuyChon = _context.TuyChonSanPham
                .Include(t => t.SanPham)
                .FirstOrDefault(t => t.MaTuyChon == optionId.Value);

            if (tuyChon == null)
            {
                return NotFound(); // hoặc RedirectToAction("Index") kèm thông báo lỗi
            }

            ViewBag.TuyChonInfo = tuyChon;

            var model = new ChiTietTuyChon { MaTuyChon = optionId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChiTiet(ChiTietTuyChon chiTiet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chiTiet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã tạo chi tiết tùy chọn thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ khi tạo chi tiết tùy chọn!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult EditChiTiet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chiTiet = _context.ChiTietTuyChon
                .Include(c => c.TuyChonSanPham)
                .ThenInclude(t => t.SanPham)
                .FirstOrDefault(c => c.MaChiTietTuyChon == id);

            if (chiTiet == null)
            {
                return NotFound();
            }

            ViewBag.TuyChonInfo = chiTiet.TuyChonSanPham;

            return View(chiTiet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChiTiet(int id, ChiTietTuyChon chiTiet)
        {
            if (id != chiTiet.MaChiTietTuyChon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTiet);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật chi tiết tùy chọn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTietExists(chiTiet.MaChiTietTuyChon))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi khi cập nhật chi tiết tùy chọn!";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ khi cập nhật!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChiTiet(int id)
        {
            var chiTiet = await _context.ChiTietTuyChon.FindAsync(id);
            if (chiTiet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chi tiết tùy chọn cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            _context.ChiTietTuyChon.Remove(chiTiet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa chi tiết tùy chọn thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool ChiTietExists(int id)
        {
            return _context.ChiTietTuyChon.Any(e => e.MaChiTietTuyChon == id);
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPham.Any(e => e.MaSanPham == id);
        }
    }
}