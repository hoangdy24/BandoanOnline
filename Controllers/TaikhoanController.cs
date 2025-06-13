using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QlW_BandoanOnline.Services;
using MailKit.Net.Smtp;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Models.ViewModels;
using QlW_BandoanOnline.Repository;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace QlW_BandoanOnline.Controllers
{
    public class TaikhoanController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        public TaikhoanController(
        UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,DataContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Dangnhap");
            }

            // Kiểm tra và đồng bộ thông tin KhachHang nếu cần
            await SyncKhachHangRecord(user);

            return View(user);
        }

        private async Task SyncKhachHangRecord(ApplicationUser user)
        {
            var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.UserId == user.Id);
            if (khachHang == null)
            {
                // Tạo mới bản ghi KhachHang nếu chưa có
                khachHang = new KhachHang
                {
                    UserId = user.Id,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    SoDienThoai = user.PhoneNumber,
                    DiaChi = user.DiaChi,
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };
                _context.KhachHang.Add(khachHang);
            }
            else
            {
                // Cập nhật thông tin nếu có thay đổi
                khachHang.HoTen = user.HoTen;
                khachHang.SoDienThoai = user.PhoneNumber;
                khachHang.DiaChi = user.DiaChi;
                _context.KhachHang.Update(khachHang);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> SyncAllUsers()
        {
            if (!User.IsInRole("Admin")) // Chỉ admin mới được chạy
            {
                return Forbid();
            }

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                await SyncKhachHangRecord(user);
            }

            TempData["ToastMessage"] = "Đã đồng bộ tất cả user với bảng KhachHang";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Quenmatkhau()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quenmatkhau(QuenMatKhauViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["ToastMessage"] = "Nếu email tồn tại, chúng tôi đã gửi link reset.";
                TempData["ToastType"] = "info";

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("Quenmatkhau");
                }

                // Tạo token với thời hạn ngắn (ví dụ: 15 phút)
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Mã hóa token trước khi gửi
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var resetLink = Url.Action(
                    "ResetPassword",
                    "Taikhoan",
                    new { token = encodedToken, email = user.Email },
                    Request.Scheme);

                try
                {
                    await _emailService.SendEmailAsync(
                        model.Email,
                        "Đặt lại mật khẩu",
                        $"Vui lòng click vào link sau trong vòng 15 phút để đặt lại mật khẩu: <a href='{HtmlEncoder.Default.Encode(resetLink)}'>Đặt lại mật khẩu</a>");
                }
                catch
                {
                    TempData["ToastMessage"] = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
                    TempData["ToastType"] = "error";
                }

                return RedirectToAction("Quenmatkhau");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Quenmatkhau");
            }

            try
            {
                // Giải mã token
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var model = new ResetMatKhauViewModel
                {
                    Token = decodedToken,
                    Email = email
                };
                return View(model);
            }
            catch
            {
                TempData["ToastMessage"] = "Link đặt lại mật khẩu không hợp lệ.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Quenmatkhau");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                TempData["ToastMessage"] = "Mật khẩu đã được đặt lại thành công.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Dangnhap");
            }

            // Ensure the token is properly decoded
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
            {
                TempData["ToastMessage"] = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập ngay bây giờ.";
                TempData["ToastType"] = "success";
                return RedirectToAction("Dangnhap");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Add specific error message for expired token
            if (result.Errors.Any(e => e.Code == "InvalidToken"))
            {
                ModelState.AddModelError(string.Empty, "Link đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu lại.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Dangky()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dangky(DangkyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    DiaChi = model.DiaChi,
                    PhoneNumber = model.SoDienThoai
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "KhachHang");

                    // Tạo bản ghi KhachHang
                    var khachHang = new KhachHang
                    {
                        UserId = user.Id,
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = model.SoDienThoai,
                        DiaChi = model.DiaChi,
                        NgayTao = DateTime.Now,
                        TrangThai = true
                    };

                    _context.KhachHang.Add(khachHang);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["ToastMessage"] = "Đăng ký tài khoản thành công!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Dangnhap(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dangnhap(DangnhapViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var khachHang = await _context.KhachHang.FirstOrDefaultAsync(k => k.UserId == user.Id);
                    if (khachHang != null && !khachHang.TrangThai)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["ToastMessage"] = "Đăng nhập thành công!";
                    TempData["ToastType"] = "success";
                    return await RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Dangnhap");
            }
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Dangnhap");
                }

                // Cập nhật thông tin user
                user.HoTen = model.HoTen;
                user.PhoneNumber = model.PhoneNumber;
                user.NgaySinh = model.NgaySinh;
                user.DiaChi = model.DiaChi;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Đồng bộ với bảng KhachHang
                    await SyncKhachHangRecord(user);

                    TempData["ToastMessage"] = "Thông tin đã được cập nhật thành công!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Dangnhap");
            }

            // ChangePasswordAsync changes the user password
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["ToastMessage"] = "Mật khẩu đã được thay đổi thành công!";
                TempData["ToastType"] = "success";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dangxuat()
        {
            await _signInManager.SignOutAsync();
            TempData["ToastMessage"] = "Đăng xuất thành công!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RedirectToLocal(string returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Home", new { area = "Admin" });

                else if (roles.Contains("QuanLy"))
                    return RedirectToAction("ChoXacNhan", "DonHangQuanLy", new { area = "QuanLy" });

                else if (roles.Contains("NhanVien"))
                    return RedirectToAction("EmployeeDashboard", "NhanVienGiaoHang", new { area = "NhanVien" });
            }

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
