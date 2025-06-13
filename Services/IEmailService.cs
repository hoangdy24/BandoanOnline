using System.Threading.Tasks;
namespace QlW_BandoanOnline.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
