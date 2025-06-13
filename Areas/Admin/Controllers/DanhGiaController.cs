using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using System.Linq;
using System.Threading.Tasks;
using QlW_BandoanOnline.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin")]
    public class DanhGiaController : Controller
    {
        private readonly DataContext _context;
        public DanhGiaController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, int? rating = null)
        {
            var query = _context.DanhGiaDonHang
                .Include(dg => dg.User)
                .Include(dg => dg.DonHang)
                .OrderByDescending(dg => dg.NgayDanhGia);

            if (rating.HasValue)
            {
                query = query.Where(dg => dg.Diem == rating.Value).OrderByDescending(dg => dg.NgayDanhGia);
            }

            var totalItems = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new DanhGiaViewModel
            {
                Reviews = reviews,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems
                },
                FilterRating = rating
            };

            return View(model);
        }
    }
}
