using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QlW_BandoanOnline.Hubs;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using Microsoft.EntityFrameworkCore;

[Area("NhanVien")]
[Authorize(Roles = "NhanVien")]
public class NhanVienGiaoHangController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly DataContext _context;

    public NhanVienGiaoHangController(
        UserManager<ApplicationUser> userManager,
        IHubContext<OrderHub> hubContext,
        DataContext context)
    {
        _userManager = userManager;
        _hubContext = hubContext;
        _context = context;
    }

    public async Task<IActionResult> EmployeeDashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Dangnhap", "Taikhoan", new { area = "" });
        }

        // ✅ Kiểm tra user có đúng role "NhanVien"
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("NhanVien"))
        {
            return Forbid(); // Trả về 403 nếu không đúng quyền
        }

        var nhanVien = await _context.NhanVien
            .Include(n => n.User)
            .Include(n => n.CuaHang)
            .FirstOrDefaultAsync(n => n.NhanVienId == user.Id);

        if (nhanVien == null || !nhanVien.DangLamViec)
        {
            TempData["ErrorMessage"] = "Tài khoản nhân viên không hợp lệ hoặc đã nghỉ việc.";
            return RedirectToAction("Dangnhap", "Taikhoan", new { area = "" });
        }

        ViewBag.NhanVien = nhanVien;
        ViewBag.EmployeeId = user.Id;

        var donHangs = await _context.DonHang
            .Include(d => d.User)
            .Include(d => d.ChiTietDonHang)
                .ThenInclude(ct => ct.SanPham)
            .Where(d =>
                (d.TrangThai == TrangThaiDonHang.DangChuanBi ||
                 d.TrangThai == TrangThaiDonHang.DangGiaoHang) &&
                (d.NhanVienGiaoHangId == null ||
                 d.NhanVienGiaoHangId == nhanVien.NhanVienId))
            .OrderBy(d => d.ThoiGianDatHang)
            .ToListAsync();

        return View(donHangs);
    }

    [HttpPost]
    public async Task<IActionResult> NhanDonGiaoHang(int maDonHang)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("NhanVien")) return Forbid();

        var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.NhanVienId == user.Id);
        if (nhanVien == null) return Json(new { success = false, message = "Không tìm thấy nhân viên." });

        // Thêm transaction để tránh race condition
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var donHang = await _context.DonHang
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang &&
                                        (d.NhanVienGiaoHangId == null ||
                                         d.NhanVienGiaoHangId == nhanVien.NhanVienId));

            if (donHang == null)
                return Json(new { success = false, message = "Đơn hàng không tồn tại hoặc đã được nhân viên khác nhận." });

            // Kiểm tra lại trạng thái đơn hàng
            if (donHang.TrangThai != TrangThaiDonHang.DangChuanBi)
                return Json(new { success = false, message = "Đơn hàng không ở trạng thái chờ giao." });

            // Cập nhật
            donHang.NhanVienGiaoHangId = nhanVien.NhanVienId;
            donHang.TrangThai = TrangThaiDonHang.DangGiaoHang;
            donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

            _context.Update(donHang);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.Groups("AdminGroup", "QuanLyGroup", "NhanVienGroup").SendAsync("OrderStatusChanged", new
            {
                orderId = maDonHang,
                status = "Đang giao",
                employeeName = nhanVien.HoTen,
                employeeId = nhanVien.NhanVienId
            });

            return Json(new { success = true, message = "Nhận đơn thành công" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = "Có lỗi xảy ra khi nhận đơn: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> HoanThanhGiaoHang(int maDonHang)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("NhanVien")) return Forbid();

        var nhanVien = await _context.NhanVien.FirstOrDefaultAsync(n => n.NhanVienId == user.Id);
        if (nhanVien == null) return Json(new { success = false, message = "Nhân viên không hợp lệ." });

        var donHang = await _context.DonHang.FindAsync(maDonHang);
        if (donHang == null) return Json(new { success = false, message = "Đơn hàng không tồn tại." });

        if (donHang.NhanVienGiaoHangId != nhanVien.NhanVienId)
        {
            return Json(new { success = false, message = "Bạn không được giao đơn hàng này." });
        }

        donHang.TrangThai = TrangThaiDonHang.DaGiaoThanhCong;
        donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

        _context.Update(donHang);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Groups("AdminGroup", "QuanLyGroup", "NhanVienGroup").SendAsync("OrderStatusChanged", new
        {
            orderId = maDonHang,
            status = "Đã giao thành công",
            employeeName = nhanVien.HoTen
        });

        return Json(new { success = true, message = "Đã hoàn thành đơn hàng" });
    }

    public async Task<IActionResult> ChiTietDonHang(int id)
    {
        var donHang = await _context.DonHang
            .Include(d => d.User)
            .Include(d => d.ChiTietDonHang)
            .ThenInclude(ct => ct.SanPham)
            .FirstOrDefaultAsync(d => d.MaDonHang == id);

        if (donHang == null) return NotFound();

        return View(donHang);
    }
}
