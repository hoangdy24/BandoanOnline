using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using Microsoft.AspNetCore.Authorization;
using QlW_BandoanOnline.Services;
using QlW_BandoanOnline.Repository;
using System.Collections.Generic;
using Newtonsoft.Json;
using static iTextSharp.text.pdf.AcroFields;
using Microsoft.AspNetCore.SignalR;
using QlW_BandoanOnline.Hubs;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly DataContext _context;
        private readonly VnPayService _vnPayService;
        private readonly IHubContext<OrderHub> _hubContext;

        public ThanhToanController(DataContext context, VnPayService vnPayService, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _vnPayService = vnPayService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var donHangs = await _context.DonHang
                    .Include(d => d.User)
                    .Include(d => d.ChiTietDonHang)
                    .Include(d => d.VnPayTransaction)
                    .OrderByDescending(d => d.ThoiGianDatHang)
                    .ToListAsync();

                return View(donHangs);
            }
            catch (Exception ex)
            {
                return Content("Lỗi: " + ex.Message);
            }
        }

        public IActionResult Create()
        {
            return View(new DonHang());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTenKhach,SoDienThoai,Email,DiaChiGiaoHang,GhiChuDiaChi,PhuongThucThanhToan")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin người dùng và giỏ hàng
                var userId = User.Identity.Name;
                var gioHang = await _context.GioHang
                    .Include(g => g.GioHangItem)
                    .FirstOrDefaultAsync(g => g.UserId == userId);

                if (gioHang == null || !gioHang.GioHangItem.Any())
                {
                    TempData["ToastMessage"] = "Giỏ hàng trống, không thể tạo đơn hàng";
                    TempData["ToastType"] = "error";
                    return View(donHang);
                }

                // Thiết lập thông tin đơn hàng
                donHang.UserId = userId;
                donHang.TrangThai = TrangThaiDonHang.ChoXacNhan;
                donHang.ThoiGianDatHang = DateTime.Now;

                // Chuyển đổi từ GioHangItem sang ChiTietDonHang
                donHang.ChiTietDonHang = gioHang.GioHangItem.Select(item => new ChiTietDonHang
                {
                    MaSanPham = item.MaSanPham,
                    TenSanPham = item.TenSanPham,
                    HinhAnh = item.SanPham?.HinhAnh, // Lấy từ sản phẩm nếu cần
                    DonGia = item.GiaBan,
                    SoLuong = item.SoLuong,
                    GhiChu = item.GhiChu,
                    TuyChonThem = item.TuyChon // Giữ nguyên định dạng JSON
                }).ToList();

                _context.Add(donHang);
                _context.Add(donHang);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.Groups("AdminGroup", "QuanLyGroup").SendAsync("NewOrderReceived",
                    new
                    {
                        OrderId = donHang.MaDonHang,
                        PublicOrderId = donHang.MaDonHangPublic,
                        CustomerName = donHang.HoTenKhach,
                        OrderTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        TotalAmount = donHang.TongTien
                    });

                // Xóa giỏ hàng sau khi tạo đơn thành công
                _context.GioHangItem.RemoveRange(gioHang.GioHangItem);
                _context.GioHang.Remove(gioHang);
                await _context.SaveChangesAsync();

                // Xử lý thanh toán nếu là VNPay
                if (donHang.PhuongThucThanhToan == PhuongThucThanhToan.VNPay)
                {
                    if (donHang.TongTien < 5000)
                    {
                        TempData["ToastMessage"] = "Số tiền thanh toán tối thiểu là 5,000 VND";
                        TempData["ToastType"] = "error";
                        return View(donHang);
                    }

                    var vnPayTransaction = new VnPayTransaction
                    {
                        MaDonHang = donHang.MaDonHang,
                        Amount = donHang.TongTien,
                        CreatedDate = DateTime.Now,
                        OrderInfo = $"Thanh toán đơn hàng #{donHang.MaDonHang}"
                    };

                    _context.Add(vnPayTransaction);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("PaymentWithVnPay", new { id = donHang.MaDonHang });
                }
                else if (donHang.PhuongThucThanhToan == PhuongThucThanhToan.TienMat)
                {
                    donHang.ThoiGianThanhToan = DateTime.Now;
                    donHang.TrangThai = TrangThaiDonHang.ChoXacNhan;
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("PaymentSuccess", new { id = donHang.MaDonHang });
                }
            }
            return View(donHang);
        }

        [HttpGet("PaymentWithVnPay/{id}")]
        public IActionResult PaymentWithVnPay(int id)
        {
            var donHang = _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null || donHang.TongTien <= 0)
            {
                return NotFound();
            }

            var paymentUrl = _vnPayService.CreatePaymentUrl(
                donHang.MaDonHang,
                donHang.TongTien,
                $"Thanh toan don hang #{donHang.MaDonHang}"
            );

            return Redirect(paymentUrl);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHang
                .Include(d => d.User)
                .Include(d => d.ChiTietDonHang)
                .Include(d => d.VnPayTransaction)
                .FirstOrDefaultAsync(m => m.MaDonHang == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        [AllowAnonymous]
        [HttpGet("/ThanhToan/VnPayCallback")]
        public async Task<IActionResult> VnPayCallback()
        {
            try
            {
                if (!Request.Query.Any())
                {
                    return RedirectToAction("PaymentFailed", new { message = "Không có dữ liệu từ VNPay" });
                }

                var response = _vnPayService.ProcessCallback(Request.Query);

                if (!response.IsValid)
                {
                    return RedirectToAction("PaymentFailed", new { message = "Chữ ký không hợp lệ" });
                }

                if (!int.TryParse(response.OrderId, out int orderId))
                {
                    return RedirectToAction("PaymentFailed", new { message = "Mã đơn hàng không hợp lệ" });
                }
                var donHang = await _context.DonHang
                            .Include(d => d.VnPayTransaction)
                            .Include(d => d.ChiTietDonHang)
                            .FirstOrDefaultAsync(d => d.MaDonHang == orderId);

                if (donHang == null)
                {
                    return RedirectToAction("PaymentFailed", new
                    {
                        message = $"Không tìm thấy đơn hàng #{orderId}",
                        orderId = orderId
                    });
                }

                // Tính toán lại tổng tiền từ chi tiết đơn hàng
                var tongTienHienTai = donHang.ChiTietDonHang?.Sum(ct => ct.ThanhTien) ?? 0;

                if (Math.Abs(response.Amount - donHang.TongTien) > 0.01m)
                {
                    // Sửa dòng này - truyền message qua parameter chứ không phải tên view
                    return RedirectToAction("PaymentFailed", new
                    {
                        message = $"Số tiền không khớp: {response.Amount} vs {donHang.TongTien}",
                        orderId = orderId.ToString()
                    });
                }

                var vnPayTransaction = donHang.VnPayTransaction ?? new VnPayTransaction
                {
                    MaDonHang = donHang.MaDonHang
                };

                vnPayTransaction.MaGiaoDich = response.TransactionId;
                vnPayTransaction.BankCode = response.BankCode;
                vnPayTransaction.ResponseCode = response.ResponseCode;
                vnPayTransaction.TransactionStatus = response.TransactionStatus;
                vnPayTransaction.PayDate = DateTime.Now;
                vnPayTransaction.SecureHash = Request.Query["vnp_SecureHash"];

                if (donHang.VnPayTransaction == null)
                {
                    _context.Add(vnPayTransaction);
                }

                if (response.IsSuccess)
                {
                    donHang.DaThanhToan = true;
                    donHang.ThoiGianThanhToan = DateTime.Now;
                    donHang.TrangThai = TrangThaiDonHang.ChoXacNhan;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("PaymentSuccess", new { id = orderId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("PaymentFailed", new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        public IActionResult PaymentSuccess(int id)
        {
            var donHang = _context.DonHang.FirstOrDefault(d => d.MaDonHang == id);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewBag.OrderId = id;
            ViewBag.MaDonHangPublic = donHang.MaDonHangPublic;
            return View(donHang);
        }

        public IActionResult PaymentFailed(string message, string orderId = null)
        {
            ViewBag.ErrorMessage = message;
            ViewBag.OrderId = orderId;
            return View();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHang
                .FirstOrDefaultAsync(m => m.MaDonHang == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: DonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            _context.DonHang.Remove(donHang);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, TrangThaiDonHang status)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }

            donHang.TrangThai = status;
            donHang.ThoiGianCapNhatTrangThai = DateTime.Now;

            if (status == TrangThaiDonHang.DaGiaoThanhCong ||
                (donHang.PhuongThucThanhToan == PhuongThucThanhToan.TienMat && status == TrangThaiDonHang.ChoXacNhan))
            {
                donHang.DaThanhToan = true;
                donHang.ThoiGianThanhToan = DateTime.Now;
            }

            _context.Update(donHang);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHang.Any(e => e.MaDonHang == id);
        }
    }
}