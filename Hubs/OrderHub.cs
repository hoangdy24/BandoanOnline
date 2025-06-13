using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace QlW_BandoanOnline.Hubs
{
    [Authorize(Roles = "Admin,QuanLy,NhanVien")]
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        // Tham gia nhóm theo role
        public async Task JoinRoleGroup()
        {
            var user = Context.User;

            if (user.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
                _logger.LogInformation($"Connection {Context.ConnectionId} joined AdminGroup");
            }

            if (user.IsInRole("QuanLy"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "QuanLyGroup");
                _logger.LogInformation($"Connection {Context.ConnectionId} joined QuanLyGroup");
            }

            if (user.IsInRole("NhanVien"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "NhanVienGroup");
                _logger.LogInformation($"Connection {Context.ConnectionId} joined NhanVienGroup");
            }
        }

        // Tham gia nhóm theo đơn hàng
        public async Task JoinOrderGroup(string orderId)
        {
            var groupName = $"order-{orderId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Connection {Context.ConnectionId} joined order group {groupName}");
        }

        public async Task JoinNewOrdersGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "NewOrders");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined NewOrders group");
        }

        // Rời nhóm theo đơn hàng
        public async Task LeaveOrderGroup(string orderId)
        {
            var groupName = $"order-{orderId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Connection {Context.ConnectionId} left order group {groupName}");
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            var user = Context.User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                _logger.LogInformation($"Authenticated user: {user.Identity.Name}");
                await JoinRoleGroup();
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyNewOrder(object orderInfo)
        {
            await Clients.Groups("AdminGroup", "QuanLyGroup").SendAsync("NewOrderReceived", orderInfo);
            _logger.LogInformation("NewOrderReceived event sent to AdminGroup and QuanLyGroup");
        }

        public async Task NotifyOrderToPrepare(object orderInfo)
        {
            await Clients.Group("NhanVienGroup").SendAsync("NewOrderToPrepare", orderInfo);
            _logger.LogInformation($"Đã gửi thông báo đơn hàng cần chuẩn bị: {orderInfo}");
        }
    }
}
