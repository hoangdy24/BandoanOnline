using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QlW_BandoanOnline.Hubs;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class GioHangController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<GioHangHub> _hubContext;

        public GioHangController(
            DataContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<GioHangHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: /GioHang
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var gioHang = await LayGioHang(user.Id);
            return View(gioHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDon(ThemGioHangViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var gioHang = await LayGioHang(user.Id);
                var sanPham = await _context.SanPham.FindAsync(model.MaSanPham);
                if (sanPham == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                // Kiểm tra sản phẩm đã có trong giỏ chưa
                var existingItem = gioHang.GioHangItem.FirstOrDefault(i =>
                    i.MaSanPham == model.MaSanPham &&
                    i.TuyChon == model.TuyChonJson);

                if (existingItem != null)
                {
                    existingItem.SoLuong += model.SoLuong; // Tăng số lượng nếu đã có
                }
                else
                {
                    gioHang.GioHangItem.Add(new GioHangItem
                    {
                        MaSanPham = model.MaSanPham,
                        TenSanPham = sanPham.TenSanPham,
                        GiaBan = sanPham.GiaBan,
                        SoLuong = model.SoLuong,
                        TuyChon = model.TuyChonJson,
                        MaGioHang = gioHang.MaGioHang
                    });
                }

                await _context.SaveChangesAsync();

                var tongSoLuong = gioHang.GioHangItem.Sum(i => i.SoLuong);
                await _hubContext.Clients.User(user.Id).SendAsync("CapNhatSoLuongGioHang", tongSoLuong);

                return Json(new
                {
                    success = true,
                    message = $"Đã thêm {sanPham.TenSanPham} vào giỏ hàng",
                    cartCount = tongSoLuong,
                    productName = sanPham.TenSanPham
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Chưa đăng nhập để thêm vào giỏ hàng"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(ThemGioHangViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

                var gioHang = await LayGioHang(user.Id);
                var sanPham = await _context.SanPham.FindAsync(model.MaSanPham);
                if (sanPham == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

                // Kiểm tra sản phẩm đã có trong giỏ chưa
                var existingItem = gioHang.GioHangItem.FirstOrDefault(i =>
                    i.MaSanPham == model.MaSanPham &&
                    i.TuyChon == model.TuyChonJson);

                if (existingItem != null)
                {
                    existingItem.SoLuong += model.SoLuong; // Tăng số lượng nếu đã có
                }
                else
                {
                    gioHang.GioHangItem.Add(new GioHangItem
                    {
                        MaSanPham = model.MaSanPham,
                        TenSanPham = sanPham.TenSanPham,
                        GiaBan = sanPham.GiaBan,
                        SoLuong = model.SoLuong,
                        TuyChon = model.TuyChonJson,
                        GhiChu = model.GhiChu,
                        MaGioHang = gioHang.MaGioHang
                    });
                }

                gioHang.NgayCapNhat = DateTime.Now;
                await _context.SaveChangesAsync();

                var tongSoLuong = gioHang.GioHangItem.Sum(i => i.SoLuong);
                await _hubContext.Clients.User(user.Id).SendAsync("CapNhatSoLuongGioHang", tongSoLuong);

                return Json(new
                {
                    success = true,
                    message = $"Đã thêm {sanPham.TenSanPham} vào giỏ hàng",
                    cartCount = tongSoLuong,
                    productName = sanPham.TenSanPham
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapNhat(int MaSanPham, int soLuong, string actionType)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.GioHangItem
                .Include(i => i.GioHang)
                .Include(i => i.SanPham)
                .FirstOrDefaultAsync(i => i.MaSanPham == MaSanPham && i.GioHang.UserId == user.Id);

            if (item == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Xử lý đặc biệt cho sản phẩm thêm từ _ThemGioHangDon
            var isDefaultOption = item.TuyChon == JsonConvert.SerializeObject(new[] {
        new { TenTuyChon = "Mặc định", LuaChon = "Không có", GiaThem = 0 }
    });

            string message = "";
            bool isRemoved = false;
            var giaThem = 0m;

            if (!string.IsNullOrEmpty(item.TuyChon) && !isDefaultOption)
            {
                var tuyChonList = JsonConvert.DeserializeObject<List<dynamic>>(item.TuyChon);
                giaThem = tuyChonList?.Sum(o => (decimal)(o?.GiaThem ?? 0)) ?? 0m;
            }

            switch (actionType)
            {
                case "increase":
                    item.SoLuong += 1;
                    message = $"Đã tăng số lượng {item.TenSanPham} lên {item.SoLuong}";
                    break;
                case "decrease":
                    item.SoLuong -= 1;
                    message = $"Đã giảm số lượng {item.TenSanPham} xuống {item.SoLuong}";
                    break;
                case "set":
                    item.SoLuong = soLuong;
                    message = $"Đã cập nhật số lượng {item.TenSanPham} thành {item.SoLuong}";
                    break;
            }

            if (item.SoLuong <= 0)
            {
                _context.GioHangItem.Remove(item);
                message = $"Đã xóa {item.TenSanPham} khỏi giỏ hàng";
                isRemoved = true;
            }

            await _context.SaveChangesAsync();

            var gioHang = await LayGioHang(user.Id);
            var tongSoLuong = gioHang.GioHangItem.Sum(i => i.SoLuong);
            await _hubContext.Clients.User(user.Id).SendAsync("CapNhatSoLuongGioHang", tongSoLuong);

            return Json(new
            {
                success = true,
                message,
                isRemoved,
                productName = item.TenSanPham,
                newQuantity = item.SoLuong,
                newSubtotal = (item.GiaBan + giaThem) * item.SoLuong,
                cartCount = tongSoLuong
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.GioHangItem
                .Include(i => i.GioHang)
                .Include(i => i.SanPham)
                .FirstOrDefaultAsync(i => i.MaSanPham == id && i.GioHang.UserId == user.Id);

            if (item != null)
            {
                _context.GioHangItem.Remove(item);
                await _context.SaveChangesAsync();

                var gioHang = await LayGioHang(user.Id);
                var tongSoLuong = gioHang.GioHangItem.Sum(i => i.SoLuong);
                await _hubContext.Clients.User(user.Id).SendAsync("CapNhatSoLuongGioHang", tongSoLuong);

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa {item.TenSanPham} khỏi giỏ hàng",
                    cartCount = tongSoLuong
                });
            }

            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCa()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var gioHang = await LayGioHang(user.Id);

                if (gioHang != null && gioHang.GioHangItem.Any())
                {
                    _context.GioHangItem.RemoveRange(gioHang.GioHangItem);
                    gioHang.NgayCapNhat = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Gửi thông báo cập nhật qua SignalR
                    await _hubContext.Clients.User(user.Id).SendAsync("CapNhatSoLuongGioHang", 0);

                    return Json(new
                    {
                        success = true,
                        message = "Đã xóa tất cả sản phẩm khỏi giỏ hàng"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Giỏ hàng đã trống"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi xóa giỏ hàng: " + ex.Message
                });
            }
        }

        private async Task<GioHang> LayGioHang(string userId)
        {
            var gioHang = await _context.GioHang
                .Include(g => g.GioHangItem)
                .ThenInclude(i => i.SanPham)
                .FirstOrDefaultAsync(g => g.UserId == userId);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    UserId = userId,
                    GioHangItem = new List<GioHangItem>(),
                    NgayCapNhat = DateTime.Now
                };
                _context.GioHang.Add(gioHang);
                await _context.SaveChangesAsync();
            }
            return gioHang;
        }

        [HttpGet]
        public async Task<IActionResult> GetTongSoLuong()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { tongSoLuong = 0 });

            var tongSoLuong = await _context.GioHangItem
                .Where(x => x.GioHang.UserId == user.Id)
                .SumAsync(x => x.SoLuong);

            return Json(new { tongSoLuong });
        }
    }
}