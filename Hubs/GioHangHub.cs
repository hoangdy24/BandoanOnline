using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace QlW_BandoanOnline.Hubs
{
    public class GioHangHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GioHangHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(Context.User);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{user.Id}");
            }
            await base.OnConnectedAsync();
        }

        public async Task JoinUserGroup()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(Context.User);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{user.Id}");
            }
        }
    }
}
