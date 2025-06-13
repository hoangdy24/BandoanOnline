using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text.Json;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using TaoNhanVienViewModel = QlW_BandoanOnline.Areas.Admin.Models.ViewModels.TaoNhanVienViewModel;
using Org.BouncyCastle.Crypto.Generators;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NhanVienAdminController : Controller
    {
        private readonly DataContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<NhanVienAdminController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public NhanVienAdminController(DataContext context, ILogger<NhanVienAdminController> logger)
        {
            _context = context;
            _roleManager = context.GetService<RoleManager<IdentityRole>>();
            _logger = logger;
            _logger.LogInformation("NhanVienController initialized");
            _userManager = context.GetService<UserManager<ApplicationUser>>();
        }
        public async Task<IActionResult> Index(string filterPosition, string filterStatus, string filterDepartment, DateTime? filterJoinDate)
        {
            var employees = _context.NhanVien
                .Include(n => n.CuaHang)
                .Include(n => n.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filterPosition))
            {
                employees = employees.Where(e => e.ChucVu.Contains(filterPosition));
            }

            if (!string.IsNullOrEmpty(filterStatus))
            {
                bool isActive = filterStatus == "active";
                employees = employees.Where(e => e.DangLamViec == isActive);
            }

            if (!string.IsNullOrEmpty(filterDepartment))
            {
                // Assuming CuaHang has a TenCuaHang property that represents department
                employees = employees.Where(e => e.CuaHang.TenCuaHang.Contains(filterDepartment));
            }

            if (filterJoinDate.HasValue)
            {
                employees = employees.Where(e => e.NgayBatDauLam.Date == filterJoinDate.Value.Date);
            }

            // Pass filter values to view to maintain filter state
            ViewBag.FilterPosition = filterPosition;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.FilterDepartment = filterDepartment;
            ViewBag.FilterJoinDate = filterJoinDate?.ToString("yyyy-MM-dd");

            return View(await employees.ToListAsync());
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var employees = await _context.NhanVien.ToListAsync();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Nhân viên");

                // Add header row
                worksheet.Cells[1, 1].Value = "Mã NV";
                worksheet.Cells[1, 2].Value = "Họ tên";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Số điện thoại";
                worksheet.Cells[1, 5].Value = "Vị trí";
                worksheet.Cells[1, 6].Value = "Ngày vào làm";
                worksheet.Cells[1, 7].Value = "Trạng thái";

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add data rows
                for (int i = 0; i < employees.Count; i++)
                {
                    var emp = employees[i];
                    worksheet.Cells[i + 2, 1].Value = emp.MaNhanVien;
                    worksheet.Cells[i + 2, 2].Value = emp.HoTen;
                    worksheet.Cells[i + 2, 3].Value = emp.Email;
                    worksheet.Cells[i + 2, 4].Value = emp.SoDienThoai;
                    worksheet.Cells[i + 2, 5].Value = emp.ChucVu;
                    worksheet.Cells[i + 2, 6].Value = emp.NgayBatDauLam.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 7].Value = emp.DangLamViec ? "Đang làm việc" : "Đã nghỉ việc";
                }

                // Auto fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return the Excel file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public async Task<IActionResult> ExportToPDF()
        {
            var employees = await _context.NhanVien.ToListAsync();

            using (var memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                var baseFont = BaseFont.CreateFont(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\arialuni.ttf",
                                  BaseFont.IDENTITY_H,
                                  BaseFont.EMBEDDED);
                var titleFont = new Font(baseFont, 18, Font.BOLD);
                var headerFont = new Font(baseFont, 10, Font.BOLD);
                var cellFont = new Font(baseFont, 10, Font.NORMAL);

                // Tiêu đề
                document.Add(new Paragraph("DANH SÁCH NHÂN VIÊN", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                });

                // Ngày xuất
                document.Add(new Paragraph($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}", cellFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 20
                });

                // Tạo bảng
                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 2.5f, 2.5f, 2f, 2f, 2f, 1.5f });

                // Tiêu đề cột
                table.AddCell(new Phrase("Mã NV", headerFont));
                table.AddCell(new Phrase("Họ tên", headerFont));
                table.AddCell(new Phrase("Email", headerFont));
                table.AddCell(new Phrase("Số điện thoại", headerFont));
                table.AddCell(new Phrase("Vị trí", headerFont));
                table.AddCell(new Phrase("Ngày vào làm", headerFont));
                table.AddCell(new Phrase("Trạng thái", headerFont));

                // Dữ liệu
                foreach (var emp in employees)
                {
                    table.AddCell(new Phrase(emp.MaNhanVien ?? "", cellFont));
                    table.AddCell(new Phrase(emp.HoTen ?? "", cellFont));
                    table.AddCell(new Phrase(emp.Email ?? "", cellFont));
                    table.AddCell(new Phrase(emp.SoDienThoai ?? "", cellFont));
                    table.AddCell(new Phrase(emp.ChucVu ?? "", cellFont));
                    table.AddCell(new Phrase(emp.NgayBatDauLam.ToString("dd/MM/yyyy"), cellFont));
                    table.AddCell(new Phrase(emp.DangLamViec ? "Đang làm việc" : "Đã nghỉ việc", cellFont));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf",
                          $"DanhSachNhanVien_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
        }


        // Hàm hỗ trợ thêm ô vào bảng
        private void AddCell(PdfPTable table, string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            table.AddCell(cell);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanVien
                .Include(n => n.CuaHang)
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.NhanVienId == id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

    public async Task<IActionResult> Create()
    {
        var model = new TaoNhanVienViewModel
        {
            CuaHangOptions = new SelectList(await _context.CuaHang.ToListAsync(), "CuaHangId", "TenCuaHang"),
            RoleOptions = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name")
        };
        return View(model);
    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoNhanVienViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu là bắt buộc");
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.SoDienThoai
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

                    var nhanVien = new NhanVien
                    {
                        NhanVienId = user.Id,
                        MaNhanVien = model.MaNhanVien,
                        HoTen = model.HoTen,
                        SoDienThoai = model.SoDienThoai,
                        Email = model.Email,
                        CCCD = model.CCCD,
                        ChucVu = model.ChucVu,
                        CuaHangId = model.CuaHangId,
                        NgayBatDauLam = model.NgayBatDauLam,
                        DangLamViec = model.DangLamViec
                    };

                    _context.Add(nhanVien);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                AddErrors(result);
            }

            model.CuaHangOptions = new SelectList(await _context.CuaHang.ToListAsync(), "CuaHangId", "TenCuaHang");
            model.RoleOptions = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        // GET: Admin/NhanVienAdmin/Edit/5
        public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var nhanVien = await _context.NhanVien
            .Include(n => n.User)
            .Include(n => n.CuaHang)
            .FirstOrDefaultAsync(n => n.NhanVienId == id);

        if (nhanVien == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(nhanVien.User);

        var model = new TaoNhanVienViewModel
        {
            NhanVienId = nhanVien.NhanVienId,
            MaNhanVien = nhanVien.MaNhanVien,
            HoTen = nhanVien.HoTen,
            SoDienThoai = nhanVien.SoDienThoai,
            Email = nhanVien.Email,
            CCCD = nhanVien.CCCD,
            ChucVu = nhanVien.ChucVu,
            CuaHangId = nhanVien.CuaHangId,
            NgayBatDauLam = nhanVien.NgayBatDauLam,
            DangLamViec = nhanVien.DangLamViec,
            CuaHangOptions = new SelectList(await _context.CuaHang.ToListAsync(), "CuaHangId", "TenCuaHang"),
            RoleOptions = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name"),
            Role = userRoles.FirstOrDefault()
        };

        return View(model);
    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TaoNhanVienViewModel model)
        {
            if (id != model.NhanVienId) return NotFound();

            // Tắt validation cho Password và ConfirmPassword khi chỉnh sửa
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (ModelState.IsValid)
            {
                try
                {
                    var nhanVien = await _context.NhanVien.FindAsync(id);
                    if (nhanVien == null) return NotFound();

                    // Cập nhật các thông tin khác
                    nhanVien.HoTen = model.HoTen;
                    nhanVien.SoDienThoai = model.SoDienThoai;
                    nhanVien.CCCD = model.CCCD;
                    nhanVien.ChucVu = model.ChucVu;
                    nhanVien.CuaHangId = model.CuaHangId;
                    nhanVien.NgayBatDauLam = model.NgayBatDauLam;
                    nhanVien.DangLamViec = model.DangLamViec;

                    var user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        user.Email = model.Email;
                        user.UserName = model.Email;
                        user.PhoneNumber = model.SoDienThoai;

                        // Chỉ đổi mật khẩu nếu có nhập
                        if (!string.IsNullOrEmpty(model.Password))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            await _userManager.ResetPasswordAsync(user, token, model.Password);
                        }

                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(id))
                        return NotFound();
                    throw;
                }
            }

            model.CuaHangOptions = new SelectList(await _context.CuaHang.ToListAsync(), "CuaHangId", "TenCuaHang");
            model.RoleOptions = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model);
        }

        private bool NhanVienExists(string id)
    {
        return _context.NhanVien.Any(e => e.NhanVienId == id);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

        private async Task<IEnumerable<SelectListItem>> GetCuaHangOptions()
        {
            return await _context.CuaHang
                .Select(c => new SelectListItem
                {
                    Value = c.CuaHangId.ToString(),
                    Text = c.TenCuaHang
                })
                .ToListAsync();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // GET: NhanVien/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NhanVien
                .Include(n => n.CuaHang)
                .FirstOrDefaultAsync(m => m.NhanVienId == id);
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Xóa tài khoản người dùng thất bại.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                var nhanVien = await _context.NhanVien.FindAsync(id);
                if (nhanVien != null)
                {
                    _context.NhanVien.Remove(nhanVien);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error deleting employee.");
                TempData["ErrorMessage"] = "Nhân viên đã bị xóa hoặc thay đổi bởi người khác.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhân viên.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa nhân viên: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
