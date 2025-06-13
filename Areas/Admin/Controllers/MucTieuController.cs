using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class MucTieuController : Controller
    {
        private readonly DataContext _context;
        public  MucTieuController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var goals = await _context.MucTieuDoanhThu.FirstOrDefaultAsync();
            if (goals == null)
            {
                goals = new MucTieuDoanhThu
                {
                    RevenueTarget = goals.RevenueTarget,
                    NewCustomerTarget = goals.NewCustomerTarget,
                    OrderTarget = goals.OrderTarget,
                    PositiveReviewTarget = goals.PositiveReviewTarget
                };
                _context.MucTieuDoanhThu.Add(goals);
                await _context.SaveChangesAsync();
            }

            return View(goals);
        }

        [HttpPost]
        public async Task<IActionResult> Update(MucTieuDoanhThu model)
        {
            if (ModelState.IsValid)
            {
                var goals = await _context.MucTieuDoanhThu.FirstOrDefaultAsync();
                if (goals == null)
                {
                    _context.MucTieuDoanhThu.Add(model);
                }
                else
                {
                    goals.RevenueTarget = model.RevenueTarget;
                    goals.NewCustomerTarget = model.NewCustomerTarget;
                    goals.OrderTarget = model.OrderTarget;
                    goals.PositiveReviewTarget = model.PositiveReviewTarget;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật mục tiêu thành công!";
                return RedirectToAction("Index","Home");
            }

            return View("Index", model);
        }
    }
}
