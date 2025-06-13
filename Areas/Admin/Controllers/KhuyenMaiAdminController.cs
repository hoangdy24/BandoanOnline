using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using PagedList.Core;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class KhuyenMaiAdminController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public KhuyenMaiAdminController(DataContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> Index(int? page, string searchString, bool? isActive)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var khuyenMaisQuery = _context.KhuyenMai.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                khuyenMaisQuery = khuyenMaisQuery.Where(k =>
                    k.TenKhuyenMai.Contains(searchString) ||
                    k.MaKhuyenMaiCode.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (isActive.HasValue)
            {
                khuyenMaisQuery = khuyenMaisQuery.Where(k => k.IsActive == isActive.Value);
            }

            // Sắp xếp theo ngày bắt đầu
            khuyenMaisQuery = khuyenMaisQuery.OrderByDescending(k => k.NgayBatDau);

            // Lấy các mục đã phân trang
            var items = await khuyenMaisQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đếm tổng số bản ghi
            var totalCount = await khuyenMaisQuery.CountAsync();

            // Tạo đối tượng StaticPagedList để phân trang
            var pagedList = new StaticPagedList<KhuyenMai>(items, pageNumber, pageSize, totalCount);

            // Lưu các giá trị lọc vào ViewBag để giữ lại khi phân trang
            ViewBag.CurrentFilter = searchString;
            ViewBag.IsActive = isActive;

            return View(pagedList);
        }

        // GET: Admin/KhuyenMai/Create
        public IActionResult Create()
        {
            ViewBag.SanPhams = _context.SanPham.ToList();
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View();
        }

        // POST: Admin/KhuyenMai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMai khuyenMai, IFormFile? hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh
                if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/KhuyenMai");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + hinhAnhFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnhFile.CopyToAsync(fileStream);
                    }

                    khuyenMai.HinhAnh = uniqueFileName;
                }

                _context.Add(khuyenMai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SanPhams = _context.SanPham.ToList();
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View(khuyenMai);
        }

        // GET: Admin/KhuyenMai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khuyenMai = await _context.KhuyenMai.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound();
            }

            ViewBag.SanPhams = _context.SanPham.ToList();
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View(khuyenMai);
        }

        // POST: Admin/KhuyenMai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhuyenMai khuyenMai, IFormFile? hinhAnhFile)
        {
            if (id != khuyenMai.MaKhuyenMai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh mới nếu có
                    if (hinhAnhFile != null && hinhAnhFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images/KhuyenMai");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Xóa hình ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(khuyenMai.HinhAnh))
                        {
                            var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, khuyenMai.HinhAnh.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + hinhAnhFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await hinhAnhFile.CopyToAsync(fileStream);
                        }

                        khuyenMai.HinhAnh = uniqueFileName;
                    }

                    _context.Update(khuyenMai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhuyenMaiExists(khuyenMai.MaKhuyenMai))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SanPhams = _context.SanPham.ToList();
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View(khuyenMai);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khuyenMai = await _context.KhuyenMai
                .FirstOrDefaultAsync(m => m.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            return View(khuyenMai); // This will render the delete confirmation view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Find the KhuyenMai record by id
            var khuyenMai = await _context.KhuyenMai.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa hình ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(khuyenMai.HinhAnh))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images/KhuyenMai", khuyenMai.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove the KhuyenMai record from the database
                _context.KhuyenMai.Remove(khuyenMai);
                await _context.SaveChangesAsync();

                // Set success message to be shown on the redirected page
                TempData["SuccessMessage"] = "Khuyến mãi đã được xóa thành công!";
            }
            catch
            {
                // In case of error, log the exception or handle as needed
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra trong quá trình xóa. Vui lòng thử lại!";
            }

            // Redirect to the Index page after successful deletion
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/KhuyenMai/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var khuyenMai = await _context.KhuyenMai.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound();
            }

            khuyenMai.IsActive = !khuyenMai.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { newStatus = khuyenMai.IsActive });
        }

        private bool KhuyenMaiExists(int id)
        {
            return _context.KhuyenMai.Any(e => e.MaKhuyenMai == id);
        }
    }
}