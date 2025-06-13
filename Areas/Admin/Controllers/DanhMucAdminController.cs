using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using Microsoft.AspNetCore.Authorization;

namespace QlW_BandoanOnline_Solution.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class DanhMucAdminController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DanhMucAdminController(DataContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(string search, string status, int? storeId)
        {
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StoreId = storeId?.ToString();
            ViewBag.CuaHangs = _context.CuaHang.ToList();

                var danhMucQuery = _context.DanhMuc
                    .Include(d => d.SanPham)
                    .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                danhMucQuery = danhMucQuery.Where(d => d.TenDanhMuc.Contains(search));

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "true";
                danhMucQuery = danhMucQuery.Where(d => d.IsActive == isActive);
            }

            if (storeId.HasValue)
                danhMucQuery = danhMucQuery.Where(d => d.CuaHangId == storeId);

            var model = danhMucQuery.ToList();

            return View(model);
        }
        // GET: DanhMuc/Create
        public IActionResult Create()
        {
            ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMucAdminViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var danhMuc = new DanhMuc
                    {
                        TenDanhMuc = model.TenDanhMuc,
                        MoTa = model.MoTa,
                        ThuTuHienThi = model.ThuTuHienThi,
                        IsActive = model.IsActive,
                        CuaHangId = model.CuaHangId
                    };

                    if (model.HinhAnhFile != null)
                    {
                        try
                        {
                            danhMuc.HinhAnh = await SaveImage(model.HinhAnhFile);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("HinhAnhFile", $"Lỗi khi lưu ảnh: {ex.Message}");
                            ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang", model.CuaHangId);
                            return View(model);
                        }
                    }

                    try
                    {
                        _context.Add(danhMuc);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex)
                    {
                        ModelState.AddModelError("", $"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            // Nếu có lỗi, hiển thị lại form với dữ liệu đã nhập
            ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang", model.CuaHangId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy ID danh mục";
                    return RedirectToAction(nameof(Index));
                }

                var danhMuc = await _context.DanhMuc.FindAsync(id);
                if (danhMuc == null)
                {
                    TempData["ErrorMessage"] = "Danh mục không tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                var model = new DanhMucAdminViewModel
                {
                    MaDanhMuc = danhMuc.MaDanhMuc,
                    TenDanhMuc = danhMuc.TenDanhMuc,
                    MoTa = danhMuc.MoTa,
                    HinhAnh = danhMuc.HinhAnh,
                    ThuTuHienThi = danhMuc.ThuTuHienThi,
                    IsActive = danhMuc.IsActive,
                    CuaHangId = danhMuc.CuaHangId
                };

                ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang", danhMuc.CuaHangId);
                return View(model);
            }
            catch
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang chỉnh sửa";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhMucAdminViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var danhMuc = await _context.DanhMuc.FindAsync(model.MaDanhMuc);
                    if (danhMuc == null)
                    {
                        TempData["ErrorMessage"] = "Danh mục không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }

                    // Kiểm tra tên trùng
                    var exists = await _context.DanhMuc
                        .FirstOrDefaultAsync(d => d.TenDanhMuc == model.TenDanhMuc && d.MaDanhMuc != model.MaDanhMuc);
                    if (exists != null)
                    {
                        ModelState.AddModelError("TenDanhMuc", "Tên danh mục đã tồn tại");
                        ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang", model.CuaHangId);
                        return View(model);
                    }

                    danhMuc.TenDanhMuc = model.TenDanhMuc;
                    danhMuc.MoTa = model.MoTa;
                    danhMuc.ThuTuHienThi = model.ThuTuHienThi;
                    danhMuc.IsActive = model.IsActive;
                    danhMuc.CuaHangId = model.CuaHangId;

                    if (model.HinhAnhFile != null)
                    {
                        if (!string.IsNullOrEmpty(danhMuc.HinhAnh))
                        {
                            DeleteImage(danhMuc.HinhAnh);
                        }
                        danhMuc.HinhAnh = await SaveImage(model.HinhAnhFile);
                    }

                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật danh mục";
            }

            ViewBag.CuaHangs = new SelectList(_context.CuaHang, "CuaHangId", "TenCuaHang", model.CuaHangId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var danhMuc = await _context.DanhMuc
                .Include(d => d.CuaHang)
                .FirstOrDefaultAsync(m => m.MaDanhMuc == id);

            if (danhMuc == null) return NotFound();

            return View(danhMuc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc == null) return NotFound();
            var hasProducts = await _context.SanPham.AnyAsync(p => p.MaDanhMuc == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Không thể xóa vì có sản phẩm tồn tại trong danh mục";
                return RedirectToAction(nameof(Index));
            }
            if (!string.IsNullOrEmpty(danhMuc.HinhAnh))
            {
                DeleteImage(danhMuc.HinhAnh);
            }

            _context.DanhMuc.Remove(danhMuc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMuc.Any(e => e.MaDanhMuc == id);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            // Validate file
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("File ảnh không hợp lệ hoặc rỗng");
            }

            // Validate extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException($"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
            }

            // Validate size (max 5MB)
            const int maxFileSize = 5 * 1024 * 1024;
            if (imageFile.Length > maxFileSize)
            {
                throw new InvalidOperationException($"Kích thước file quá lớn (tối đa {maxFileSize / 1024 / 1024}MB)");
            }

            // Create directory if not exists
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images", "Danhmuc");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lưu file ảnh", ex);
            }
            return uniqueFileName;
        }


        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            // Đảm bảo đường dẫn không chứa các ký tự nguy hiểm
            var safePath = imagePath.Replace("..", "").TrimStart('/');
            var fullPath = Path.Combine(_hostingEnvironment.WebRootPath, safePath);

            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                }
            }
        }
    }
}