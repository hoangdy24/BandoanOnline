using System.Linq;
using X.PagedList;
using X.PagedList.Mvc.Core;
using X.PagedList.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using X.PagedList.Extensions;

namespace QlW_BandoanOnline_Solution.Areas.Admin.Controllers
{
        [Area("Admin")]
        [Route("Admin/[controller]/[action]")]
        [Authorize(Roles = "Admin")]
        public class KhachHangAdminController : Controller
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly DataContext _context;
            private readonly IWebHostEnvironment _hostingEnvironment;

        public KhachHangAdminController(
                UserManager<ApplicationUser> userManager,
                RoleManager<IdentityRole> roleManager,
                DataContext context,
                IWebHostEnvironment hostingEnvironment)
        {
                _userManager = userManager;
                _roleManager = roleManager;
                _context = context;
                _hostingEnvironment = hostingEnvironment;
            }

        public async Task<IActionResult> Customers(int? page, string searchString, bool? status)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Query cho danh sách khách hàng
            IQueryable<KhachHang> customersQuery = _context.KhachHang
                .Include(k => k.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                customersQuery = customersQuery.Where(k =>
                    k.HoTen.Contains(searchString) ||
                    k.Email.Contains(searchString) ||
                    k.SoDienThoai.Contains(searchString));
            }

            if (status.HasValue)
            {
                customersQuery = customersQuery.Where(k => k.TrangThai == status.Value);
            }

            var customerList = await customersQuery.ToListAsync();
            var customers = customerList.ToPagedList(pageNumber, pageSize);

            // Lấy dữ liệu thống kê
            var statistics = new ThongKeTaiKhoanViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalCustomers = await _context.KhachHang.CountAsync(),
                ActiveCustomers = await _context.KhachHang.CountAsync(k => k.TrangThai),
                NewCustomersThisMonth = await _context.KhachHang
                    .CountAsync(k => k.NgayTao.Month == DateTime.Now.Month &&
                                    k.NgayTao.Year == DateTime.Now.Year)
            };

            // Tạo view model kết hợp
            var viewModel = new ThongKeDSUserViewModel
            {
                Customers = customers,
                Statistics = statistics
            };

            return View(viewModel);
        }

            // CHỈNH SỬA THÔNG TIN KHÁCH HÀNG
            [HttpGet]
            public async Task<IActionResult> EditCustomer(string id)
            {
                var customer = await _context.KhachHang
                    .Include(k => k.User)
                    .FirstOrDefaultAsync(k => k.UserId == id);

                if (customer == null)
                {
                    return NotFound();
                }

                var model = new SuaKhachHangViewModel
                {
                    UserId = customer.UserId,
                    HoTen = customer.HoTen,
                    Email = customer.Email,
                    SoDienThoai = customer.SoDienThoai,
                    DiaChi = customer.DiaChi,
                    TrangThai = customer.TrangThai
                };

                return View(model);
            }

            [HttpPost]
            public async Task<IActionResult> EditCustomer(SuaKhachHangViewModel model)
            {
                if (ModelState.IsValid)
                {
                    var customer = await _context.KhachHang.FindAsync(model.UserId);
                    if (customer == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin khách hàng
                    customer.HoTen = model.HoTen;
                    customer.Email = model.Email;
                    customer.SoDienThoai = model.SoDienThoai;
                    customer.DiaChi = model.DiaChi;
                    customer.TrangThai = model.TrangThai;

                    // Cập nhật thông tin người dùng liên quan
                    var user = await _userManager.FindByIdAsync(model.UserId);
                    if (user != null)
                    {
                        user.HoTen = model.HoTen;
                        user.DiaChi = model.DiaChi;
                        await _userManager.UpdateAsync(user);
                    }

                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("KhachHang");
                }

                return View(model);
            }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var customer = await _context.KhachHang.FindAsync(id);
                if (customer == null)
                {
                    TempData["ToastMessage"] = "Không tìm thấy khách hàng";
                    TempData["ToastType"] = "error";
                    return NotFound();
                }

                customer.TrangThai = !customer.TrangThai;

                // Also lock/unlock the user account
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    if (customer.TrangThai)
                    {
                        await _userManager.SetLockoutEndDateAsync(user, null);
                    }
                    else
                    {
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    }
                }

                _context.Update(customer);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Thay đổi trạng thái thành công";
                TempData["ToastType"] = "success";
                return Ok();
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi khi thay đổi trạng thái: " + ex.Message;
                TempData["ToastType"] = "error";
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                var customer = await _context.KhachHang.FindAsync(id);
                if (customer == null)
                {
                    TempData["ToastMessage"] = "Không tìm thấy khách hàng";
                    TempData["ToastType"] = "error";
                    return NotFound();
                }

                // Also delete the user account
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.KhachHang.Remove(customer);
                await _context.SaveChangesAsync();

                TempData["ToastMessage"] = "Xóa khách hàng thành công";
                TempData["ToastType"] = "success";
                return Ok();
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = "Lỗi khi xóa khách hàng: " + ex.Message;
                TempData["ToastType"] = "error";
                return StatusCode(500);
            }
        }
    }
}