using Microsoft.AspNetCore.Mvc;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Services;
using System.Threading.Tasks;

namespace QlW_BandoanOnline.Controllers
{
    public class LienHeController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<LienHeController> _logger;

        public LienHeController(IEmailService emailService, ILogger<LienHeController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitContactForm(
        [FromBody] ContactFormModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, toastMessage = "Dữ liệu không hợp lệ.", toastType = "error" });
                }

                // Tạo nội dung email
                var emailContent = $@"
                <h2>Thông tin liên hệ mới từ khách hàng</h2>
                <p><strong>Họ và tên:</strong> {model.FullName}</p>
                <p><strong>Email:</strong> {model.Email}</p>
                <p><strong>Số điện thoại:</strong> {model.Phone ?? "Không có"}</p>
                <p><strong>Chủ đề:</strong> {model.Subject}</p>
                <p><strong>Nội dung:</strong></p>
                <p>{model.Message}</p>
            ";

                await _emailService.SendEmailAsync(
                    email: "burgerbui008@gmail.com",
                    subject: $"Liên hệ từ khách hàng - {model.Subject}",
                    message: emailContent);

                return Json(new { success = true, toastMessage = "Cảm ơn bạn đã liên hệ!", toastType = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi form liên hệ");
                return Json(new { success = false, toastMessage = "Đã xảy ra lỗi. Vui lòng thử lại sau.", toastType = "error" });
            }
        }
    }
}