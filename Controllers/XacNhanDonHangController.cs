using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QlW_BandoanOnline.Hubs;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class XacNhanDonHangController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<XacNhanDonHangController> _logger;
        private readonly IHubContext<OrderHub> _hubContext;
        private const decimal PHI_GIAO_HANG_MAC_DINH = 20000;

        public XacNhanDonHangController(
            DataContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<XacNhanDonHangController> logger,
            IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin người dùng");
                    TempData["ToastMessage"] = "Vui lòng đăng nhập để tiếp tục";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                var gioHang = await LayGioHang(user.Id);
                var validItems = gioHang?.GioHangItem?.Where(i => i.SanPham != null).ToList();

                if (gioHang == null || validItems == null || !validItems.Any())
                {
                    TempData["ToastMessage"] = "Giỏ hàng của bạn đang trống hoặc có sản phẩm không hợp lệ";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("Index", "GioHang");
                }

                var tongTienTruocGiamGia = validItems.Sum(item => item.ThanhTien);
                var model = new XacNhanDonHangViewModel
                {
                    GioHang = gioHang,
                    TienGiaoHang = PHI_GIAO_HANG_MAC_DINH,
                    TongTienTruocGiamGia = tongTienTruocGiamGia,
                    TongTien = tongTienTruocGiamGia + PHI_GIAO_HANG_MAC_DINH,
                    HoTen = user.HoTen,
                    SoDienThoai = user.PhoneNumber,
                    Email = user.Email,
                    DiaChi = user.DiaChi
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang xác nhận đơn hàng");
                TempData["ToastMessage"] = "Có lỗi xảy ra khi tải trang xác nhận đơn hàng";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "GioHang");
            }
        }

        [HttpGet]
        public async Task<IActionResult> KiemTraKhuyenMai(string maKhuyenMai)
        {
            if (string.IsNullOrEmpty(maKhuyenMai))
            {
                return Json(new
                {
                    success = false,
                    toastMessage = "Vui lòng nhập mã khuyến mãi",
                    toastType = "error"
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    toastMessage = "Vui lòng đăng nhập",
                    toastType = "error"
                });
            }

            var khuyenMai = await _context.KhuyenMai
                .FirstOrDefaultAsync(k => k.MaKhuyenMaiCode == maKhuyenMai &&
                                          k.IsActive &&
                                          k.NgayBatDau <= DateTime.Now &&
                                          k.NgayKetThuc >= DateTime.Now);

            if (khuyenMai == null)
            {
                return Json(new
                {
                    success = false,
                    toastMessage = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn",
                    toastType = "error"
                });
            }

            if (khuyenMai.DaSuDung >= khuyenMai.SoLuong)
            {
                return Json(new
                {
                    success = false,
                    toastMessage = "Mã khuyến mãi đã hết lượt sử dụng",
                    toastType = "error"
                });
            }

            var daSuDung = await _context.DonHang
                .AnyAsync(d => d.UserId == user.Id && d.MaKhuyenMai == khuyenMai.MaKhuyenMai);

            if (daSuDung)
            {
                return Json(new
                {
                    success = false,
                    toastMessage = "Bạn đã sử dụng mã khuyến mãi này trước đây",
                    toastType = "error"
                });
            }

            return Json(new
            {
                success = true,
                toastMessage = "Mã khuyến mãi hợp lệ",
                toastType = "success",
                tenKhuyenMai = khuyenMai.TenKhuyenMai,
                giaTri = khuyenMai.LaPhanTram ? $"{khuyenMai.GiaTriGiam}%" : khuyenMai.GiaTriGiam.ToString()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApDungKhuyenMai(XacNhanDonHangViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ToastMessage"] = "Vui lòng đăng nhập để tiếp tục";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            var gioHang = await LayGioHang(user.Id);
            var validItems = gioHang?.GioHangItem?.Where(i => i.SanPham != null).ToList();

            if (gioHang == null || validItems == null || !validItems.Any())
            {
                TempData["ToastMessage"] = "Giỏ hàng của bạn đang trống";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index", "GioHang");
            }

            model.TienGiaoHang = model.TienGiaoHang <= 0 ? PHI_GIAO_HANG_MAC_DINH : model.TienGiaoHang;
            decimal tongTienTruocGiam = validItems.Sum(item => item.ThanhTien);
            model.TongTienTruocGiamGia = tongTienTruocGiam;

            if (!string.IsNullOrEmpty(model.MaKhuyenMai))
            {
                var khuyenMai = await _context.KhuyenMai
                    .FirstOrDefaultAsync(k => k.MaKhuyenMaiCode == model.MaKhuyenMai &&
                                              k.IsActive &&
                                              k.NgayBatDau <= DateTime.Now &&
                                              k.NgayKetThuc >= DateTime.Now &&
                                              k.DaSuDung < k.SoLuong);

                if (khuyenMai != null)
                {
                    decimal giamGia = khuyenMai.LaPhanTram
                        ? tongTienTruocGiam * khuyenMai.GiaTriGiam / 100
                        : khuyenMai.GiaTriGiam;
                    if (giamGia > tongTienTruocGiam)
                    {
                        model.GiamGia = 0;
                        model.ThongBaoKhuyenMai = $"Mã giảm giá {model.MaKhuyenMai} vượt quá tổng tiền. Áp dụng tối đa: {tongTienTruocGiam.ToString("N0")} VND";
                        TempData["ToastMessage"] = model.ThongBaoKhuyenMai;
                        TempData["ToastType"] = "warning";
                    }
                    else
                    {
                        model.GiamGia = giamGia;
                        model.ThongBaoKhuyenMai = $"Áp dụng thành công mã giảm giá {model.MaKhuyenMai}";
                        TempData["ToastMessage"] = model.ThongBaoKhuyenMai;
                        TempData["ToastType"] = "success";
                    }

                    model.TongTien = tongTienTruocGiam - model.GiamGia + model.TienGiaoHang;
                }
                else
                {
                    TempData["ToastMessage"] = "Mã khuyến mãi không hợp lệ";
                    TempData["ToastType"] = "error";
                }
            }
            else
            {
                model.GiamGia = 0;
                model.TongTien = tongTienTruocGiam + model.TienGiaoHang;
            }

            model.GioHang = gioHang;
            model.HoTen = user.HoTen;
            model.SoDienThoai = user.PhoneNumber;
            model.Email = user.Email;
            model.DiaChi = user.DiaChi;

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(XacNhanDonHangViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ToastMessage"] = "Vui lòng đăng nhập để tiếp tục";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                var gioHang = await LayGioHang(user.Id);
                var validItems = gioHang?.GioHangItem?.Where(i => i.SanPham != null).ToList();

                if (model == null || gioHang == null || validItems == null || !validItems.Any())
                {
                    TempData["ToastMessage"] = "Giỏ hàng của bạn đang trống";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("Index", "GioHang");
                }

                if (!ModelState.IsValid)
                {
                    model.GioHang = gioHang;
                    model.TongTienTruocGiamGia = validItems.Sum(item => item.ThanhTien);
                    model.TienGiaoHang = model.TienGiaoHang <= 0 ? PHI_GIAO_HANG_MAC_DINH : model.TienGiaoHang;
                    model.TongTien = model.TongTienTruocGiamGia - model.GiamGia + model.TienGiaoHang;
                    TempData["ToastMessage"] = "Vui lòng kiểm tra lại thông tin đơn hàng";
                    TempData["ToastType"] = "error";
                    return View("Index", model);
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        KhuyenMai khuyenMai = null;
                        if (!string.IsNullOrEmpty(model.MaKhuyenMai))
                        {
                            khuyenMai = await _context.KhuyenMai
                                .FirstOrDefaultAsync(k => k.MaKhuyenMaiCode == model.MaKhuyenMai);

                            var daSuDung = await _context.DonHang
                                .AnyAsync(d => d.UserId == user.Id && d.MaKhuyenMai == khuyenMai.MaKhuyenMai);

                            if (khuyenMai == null || daSuDung || khuyenMai.DaSuDung >= khuyenMai.SoLuong)
                            {
                                ModelState.AddModelError("", "Mã khuyến mãi không hợp lệ");
                                throw new Exception("Mã khuyến mãi không hợp lệ");
                            }
                        }

                        var tongTienTruocGiam = validItems.Sum(item => item.ThanhTien);
                        var tongTienSauGiam = tongTienTruocGiam - model.GiamGia + (model.TienGiaoHang <= 0 ? PHI_GIAO_HANG_MAC_DINH : model.TienGiaoHang);

                        var donHang = new DonHang
                        {
                            MaDonHangPublic = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                            UserId = user.Id,
                            HoTenKhach = model.HoTen,
                            SoDienThoai = model.SoDienThoai,
                            Email = model.Email,
                            DiaChiGiaoHang = model.DiaChi,
                            ThoiGianDatHang = DateTime.Now,
                            PhuongThucThanhToan = model.PhuongThucThanhToan,
                            TrangThai = TrangThaiDonHang.ChoXacNhan,
                            GhiChuDiaChi = model.GhiChu,
                            GiamGia = model.GiamGia,
                            TienGiaoHang = model.TienGiaoHang <= 0 ? PHI_GIAO_HANG_MAC_DINH : model.TienGiaoHang,
                            TongTienTruocGiamGiaDB = tongTienTruocGiam,
                            TongTienSauGiamGiaDB = tongTienSauGiam,
                            MaKhuyenMai = khuyenMai?.MaKhuyenMai,
                            ChiTietDonHang = validItems.Select(item => new ChiTietDonHang
                            {
                                MaSanPham = item.MaSanPham,
                                TenSanPham = item.TenSanPham,
                                HinhAnh = item.SanPham.HinhAnh,
                                SoLuong = item.SoLuong,
                                DonGia = item.GiaBan,
                                TuyChonThem = item.TuyChon,
                                GhiChu = item.GhiChu
                            }).ToList()
                        };

                        _context.DonHang.Add(donHang);

                        if (khuyenMai != null)
                        {
                            khuyenMai.DaSuDung++;
                            _context.Update(khuyenMai);
                        }

                        _context.GioHangItem.RemoveRange(validItems);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        await _hubContext.Clients.Groups("AdminGroup", "QuanLyGroup").SendAsync("NewOrderReceived",
                        new
                        {
                            OrderId = donHang.MaDonHang,
                            PublicOrderId = donHang.MaDonHangPublic,
                            CustomerName = donHang.HoTenKhach,
                            OrderTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            TotalAmount = donHang.TongTien
                        });

                        TempData["ToastMessage"] = $"Đặt hàng thành công! Mã đơn hàng: {donHang.MaDonHangPublic}";
                        TempData["ToastType"] = "success";

                        if (donHang.PhuongThucThanhToan == PhuongThucThanhToan.VNPay)
                        {
                            return RedirectToAction("PaymentWithVnPay", "ThanhToan", new { id = donHang.MaDonHang });
                        }
                        else
                        {
                            return RedirectToAction("PaymentSuccess", "ThanhToan", new { id = donHang.MaDonHang });
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Lỗi khi xác nhận đơn hàng: {ex.Message}");
                        model.GioHang = await LayGioHang(user.Id);
                        model.TongTienTruocGiamGia = model.GioHang.GioHangItem.Where(i => i.SanPham != null).Sum(i => i.ThanhTien);
                        model.TienGiaoHang = model.TienGiaoHang <= 0 ? PHI_GIAO_HANG_MAC_DINH : model.TienGiaoHang;
                        model.TongTien = model.TongTienTruocGiamGia - model.GiamGia + model.TienGiaoHang;
                        TempData["ToastMessage"] = $"Lỗi khi xác nhận đơn hàng: {ex.Message}";
                        TempData["ToastType"] = "error";

                        return View("Index", model);
                    }
                }
            }
            catch
            {
                TempData["ToastMessage"] = "Có lỗi nghiêm trọng xảy ra khi xử lý đơn hàng";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "GioHang");
            }
        }

        private async Task<GioHang> LayGioHang(string userId)
        {
            return await _context.GioHang
                .Include(g => g.GioHangItem)
                .ThenInclude(i => i.SanPham)
                .FirstOrDefaultAsync(g => g.UserId == userId);
        }
    }
}