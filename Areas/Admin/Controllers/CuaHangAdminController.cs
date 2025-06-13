using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class CuaHangAdminController : Controller
    {
        private readonly DataContext _context;

        public CuaHangAdminController(DataContext context)
        {
            _context = context;
        }

        // GET: CuaHang
        public async Task<IActionResult> Index(string searchString, string filterType)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            var cuaHangsQuery = _context.CuaHang.AsQueryable();

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                cuaHangsQuery = cuaHangsQuery.Where(ch =>
                    ch.TenCuaHang.Contains(searchString) ||
                    ch.DiaChi.Contains(searchString));
            }

            // Xử lý lọc
            switch (filterType)
            {
                case "open":
                    cuaHangsQuery = cuaHangsQuery.Where(ch => ch.GioMoCua <= currentTime && ch.GioDongCua >= currentTime);
                    break;
                case "closed":
                    cuaHangsQuery = cuaHangsQuery.Where(ch => ch.GioMoCua > currentTime || ch.GioDongCua < currentTime);
                    break;
                case "name":
                    cuaHangsQuery = cuaHangsQuery.OrderBy(ch => ch.TenCuaHang);
                    break;
            }

            var cuaHangs = await cuaHangsQuery.ToListAsync();

            // Tạo view model với trạng thái mở/đóng
            var viewModel = cuaHangs.Select(ch => new CuaHangViewModel
            {
                CuaHangId = ch.CuaHangId,
                TenCuaHang = ch.TenCuaHang,
                DiaChi = ch.DiaChi,
                SoDienThoai = ch.SoDienThoai,
                GioMoCua = ch.GioMoCua,
                GioDongCua = ch.GioDongCua,
                DangMoCua = currentTime >= ch.GioMoCua && currentTime <= ch.GioDongCua
            }).ToList();

            // Thống kê
            ViewBag.TongCuaHang = await _context.CuaHang.CountAsync();
            ViewBag.CuaHangDangMo = viewModel.Count(ch => ch.DangMoCua);
            ViewBag.TongNhanVien = await _context.NhanVien.CountAsync();
            ViewBag.TongDonHang = await _context.DonHang.CountAsync();

            return View(viewModel);
        }

        // GET: CuaHang/Details/5
        public async Task<IActionResult> Details(int? CuaHangId)
        {
            if (CuaHangId == null)
            {
                return NotFound();
            }

            var cuaHang = await _context.CuaHang
                .Include(ch => ch.NhanVien)
                .Include(ch => ch.DanhMuc)
                .FirstOrDefaultAsync(m => m.CuaHangId == CuaHangId);

            if (cuaHang == null)
            {
                return NotFound();
            }

            return View(cuaHang);
        }

        // GET: CuaHang/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CuaHangAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenCuaHang,DiaChi,SoDienThoai,GioMoCua,GioDongCua")] CuaHang cuaHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cuaHang);
                await _context.SaveChangesAsync();

                // Thêm thông báo thành công
                TempData["SuccessMessage"] = "Thêm cửa hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Thêm thông báo lỗi nếu có
            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập";
            return View(cuaHang);
        }

        // GET: CuaHang/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuaHang = await _context.CuaHang.FindAsync(id);
            if (cuaHang == null)
            {
                return NotFound();
            }
            return View(cuaHang);
        }

        // POST: CuaHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CuaHangId,TenCuaHang,DiaChi,SoDienThoai,GioMoCua,GioDongCua")] CuaHang cuaHang)
        {
            if (id != cuaHang.CuaHangId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuaHang);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật cửa hàng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuaHangExists(cuaHang.CuaHangId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập";
            return View(cuaHang);
        }

        // POST: CuaHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cuaHang = await _context.CuaHang.FindAsync(id);
            if (cuaHang == null)
            {
                return NotFound();
            }

            // Kiểm tra xem cửa hàng có nhân viên hay danh mục không
            var hasEmployees = await _context.NhanVien.AnyAsync(nv => nv.CuaHangId == id);
            var hasCategories = await _context.DanhMuc.AnyAsync(dm => dm.CuaHangId == id);

            if (hasEmployees || hasCategories)
            {
                TempData["ErrorMessage"] = "Không thể xóa cửa hàng vì có nhân viên hoặc danh mục liên quan.";
                return RedirectToAction(nameof(Index));
            }

            _context.CuaHang.Remove(cuaHang);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: CuaHang/Search?query=...
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var cuaHangs = await _context.CuaHang
                .Where(ch => ch.TenCuaHang.Contains(query) || ch.DiaChi.Contains(query))
                .ToListAsync();

            return View("Index", cuaHangs);
        }

        // GET: CuaHang/Filter?type=...
        public async Task<IActionResult> Filter(string type)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            IQueryable<CuaHang> query = _context.CuaHang;

            switch (type?.ToLower())
            {
                case "open":
                    query = query.Where(ch => ch.GioMoCua <= currentTime && ch.GioDongCua >= currentTime);
                    break;
                case "closed":
                    query = query.Where(ch => ch.GioMoCua > currentTime || ch.GioDongCua < currentTime);
                    break;
                case "name":
                    query = query.OrderBy(ch => ch.TenCuaHang);
                    break;
            }

            var cuaHangs = await query.ToListAsync();
            return View("Index", cuaHangs);
        }

        private bool CuaHangExists(int id)
        {
            return _context.CuaHang.Any(e => e.CuaHangId == id);
        }
    }
}