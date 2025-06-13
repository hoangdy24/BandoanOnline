
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;

namespace QlW_BandoanOnline.Controllers
{
    [Authorize]
    public class LichSuDonHangController : Controller
    {
        private readonly DataContext _context;

        public LichSuDonHangController(DataContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string status, DateTime? from_date, DateTime? to_date, string search)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var query = _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .Where(d => d.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "completed":
                        query = query.Where(d => d.TrangThai == TrangThaiDonHang.DaGiaoThanhCong);
                        break;
                    case "processing":
                        query = query.Where(d => d.TrangThai == TrangThaiDonHang.ChoXacNhan || d.TrangThai == TrangThaiDonHang.DangChuanBi);
                        break;
                    case "delivering":
                        query = query.Where(d => d.TrangThai == TrangThaiDonHang.DangGiaoHang);
                        break;
                    case "cancelled":
                        query = query.Where(d => d.TrangThai == TrangThaiDonHang.DaHuy);
                        break;
                }
            }

            if (from_date.HasValue)
            {
                query = query.Where(d => d.ThoiGianDatHang >= from_date.Value);
            }

            if (to_date.HasValue)
            {
                query = query.Where(d => d.ThoiGianDatHang <= to_date.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.MaDonHangPublic.Contains(search));
            }

            var donHangs = await query.OrderByDescending(d => d.ThoiGianDatHang).ToListAsync();
            return View(donHangs);
        }
        public ActionResult DonHang(int id)
        {
            // Lấy đơn hàng theo mã đơn hàng
            var donHang = _context.DonHang
                .Where(d => d.MaDonHang == id)
                .Include(d => d.ChiTietDonHang)
                .Include(d => d.User)
                .FirstOrDefault();

            if (donHang == null)
            {
                return View();
            }

            foreach (var ct in donHang.ChiTietDonHang)
            {
                if (!string.IsNullOrWhiteSpace(ct.TuyChonThem))
                {
                    var options = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(ct.TuyChonThem.Replace("\n", " "));
                    foreach (var opt in options)
                    {
                        string tenTuyChon = opt.TenTuyChon.ToString().Trim();
                        string luaChonId = opt.LuaChon.ToString().Trim();
                        decimal giaThem = Convert.ToDecimal(opt.GiaThem);
                        string tenLuaChon = luaChonId;
                        if (int.TryParse(luaChonId, out int chiTietId))
                        {
                            var chiTiet = _context.ChiTietTuyChon.FirstOrDefault(c => c.MaChiTietTuyChon == chiTietId);
                            if (chiTiet != null)
                                tenLuaChon = chiTiet.TenChiTiet;
                        }
                        string giaFormat = string.Format(new System.Globalization.CultureInfo("vi-VN"), "{0:c0}", giaThem);
                    }
                }
            }




            return View(donHang);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}