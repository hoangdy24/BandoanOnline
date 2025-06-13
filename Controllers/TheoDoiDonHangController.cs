using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QlW_BandoanOnline.Hubs;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class TheoDoiDonHangController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<TheoDoiDonHangController> _logger;
        private readonly IHubContext<OrderHub> _hubContext;

        public TheoDoiDonHangController(DataContext context, ILogger<TheoDoiDonHangController> logger, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var donHangs = await _context.DonHang
                    .Include(d => d.ChiTietDonHang)
                    .Include(d => d.VnPayTransaction)
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.ThoiGianDatHang)
                    .ToListAsync();

                return View(donHangs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng");
                TempData["ToastMessage"] = "Đã xảy ra lỗi khi tải danh sách đơn hàng";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult>ChiTiet(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var donHang = await _context.DonHang
                    .Include(d => d.ChiTietDonHang)
                        .ThenInclude(ct => ct.SanPham)
                    .Include(d => d.VnPayTransaction)
                    .Include(d => d.NhanVienGiaoHang)
                    .Include(d => d.NhanVienTiepNhan)
                    .FirstOrDefaultAsync(d => d.MaDonHang == id && d.UserId == userId);

                if (donHang == null)
                {
                    return NotFound();
                }

                // Xử lý tùy chọn thêm nếu cần
                foreach (var chiTiet in donHang.ChiTietDonHang)
                {
                    if (!string.IsNullOrEmpty(chiTiet.TuyChonThem))
                    {
                        try
                        {
                            var tuyChonList = JsonConvert.DeserializeObject<List<TuyChonThemModel>>(chiTiet.TuyChonThem);
                            // Có thể gán vào ViewBag nếu cần
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, $"Lỗi khi đọc tùy chọn thêm cho chi tiết đơn hàng {chiTiet.MaChiTiet}");
                        }
                    }
                }
                bool daDanhGia = _context.DanhGiaDonHang.Any(dg => dg.MaDonHang == id);
                ViewBag.DaDanhGia = daDanhGia;
                return View(donHang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xem chi tiết đơn hàng {id}");
                TempData["ToastMessage"] = "Đã xảy ra lỗi khi tải chi tiết đơn hàng";
                TempData["ToastType"] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var donHang = await _context.DonHang
                    .FirstOrDefaultAsync(d => d.MaDonHang == id && d.UserId == userId);

                if (donHang == null)
                {
                    return NotFound();
                }

                // Chỉ cho phép hủy đơn hàng ở trạng thái chờ xác nhận
                if (donHang.TrangThai != TrangThaiDonHang.ChoXacNhan)
                {
                    TempData["ToastMessage"] = "Chỉ có thể hủy đơn hàng ở trạng thái chờ xác nhận";
                    TempData["ToastType"] = "error";
                    return RedirectToAction(nameof(ChiTiet), new { id });
                }

                donHang.TrangThai = TrangThaiDonHang.DaHuy;
                donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

                _context.Update(donHang);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Sending status update for order {id} to group order-{id}");
                // Gửi thông báo real-time
                await _hubContext.Clients.Group($"order-{id}").SendAsync("OrderStatusUpdated",
                    new
                    {
                        orderId = id,
                        newStatus = donHang.TrangThai.ToString(),
                        updateTime = donHang.ThoiGianCapNhatTrangThai
                    });

                TempData["ToastMessage"] = "Đã hủy đơn hàng thành công";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hủy đơn hàng {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi hủy đơn hàng";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeuCauHoanTien(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var donHang = await _context.DonHang
                    .FirstOrDefaultAsync(d => d.MaDonHang == id && d.UserId == userId);

                if (donHang == null)
                {
                    return NotFound();
                }

                // Chỉ cho phép yêu cầu hoàn tiền với đơn hàng đã thanh toán
                if (!donHang.DaThanhToan || donHang.TrangThai != TrangThaiDonHang.DaHuy)
                {
                    TempData["ToastMessage"] = "Chỉ có thể yêu cầu hoàn tiền với đơn hàng đã thanh toán và đã hủy";
                    TempData["ToastType"] = "error";
                    return RedirectToAction(nameof(ChiTiet), new { id });
                }

                donHang.TrangThai = TrangThaiDonHang.HoanTien;
                donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

                _context.Update(donHang);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Đã gửi yêu cầu hoàn tiền thành công";
                TempData["ToastType"] = "success";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi yêu cầu hoàn tiền đơn hàng {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi yêu cầu hoàn tiền";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }
        }
    }
}