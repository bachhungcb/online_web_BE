using Api.Services;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class ChatHub : Hub
{
    private readonly PresenceTracker _tracker;
    private readonly IServiceProvider _serviceProvider;

    public ChatHub(PresenceTracker tracker, IServiceProvider serviceProvider)
    {
        _tracker = tracker;
        _serviceProvider = serviceProvider;
    }

    public override async Task OnConnectedAsync()
    {
        
        var idValue = Context.User?.FindFirst("id")?.Value;
        
        if (string.IsNullOrEmpty(idValue) || !Guid.TryParse(idValue, out Guid userId))
        {
            await base.OnConnectedAsync();
            return;
        }
        // 1. Cập nhật Tracker
        var isOnline = await _tracker.UserConnected(userId);
    
        // 2. Nếu đây là kết nối đầu tiên (vừa mới online) -> Update DB & Bắn thông báo
        if (isOnline)
        {
            // Lưu ý: Hub là Singleton/Transient, DbContext là Scoped, nên phải tạo Scope
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.UserRepository.GetById(userId);
                if (user != null)
                {
                    user.IsOnline = true;
                    unitOfWork.UserRepository.Update(user);
                    await unitOfWork.SaveChangesAsync(default);
                }
            }
    
            // 3. Gửi sự kiện cho tất cả client khác biết User này đã Online
            // (Thực tế nên chỉ gửi cho bạn bè, nhưng demo thì gửi All)
            await Clients.Others.SendAsync("UserIsOnline", userId);
        }
    
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 1. Lấy chuỗi ID ra trước
        var idValue = Context.User?.FindFirst("id")?.Value;
    
        // 2. Kiểm tra nếu null hoặc không phải Guid hợp lệ thì bỏ qua
        if (string.IsNullOrEmpty(idValue) || !Guid.TryParse(idValue, out Guid userId))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }
    
        // --- Code logic cũ của bạn ---
        // 3. Cập nhật Tracker
        var isOffline = await _tracker.UserDisconnected(userId);
    
        // 4. Nếu đã thoát hết tab -> Update DB & Bắn thông báo
        if (isOffline)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var user = await unitOfWork.UserRepository.GetById(userId);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastActive = DateTime.UtcNow;
                    unitOfWork.UserRepository.Update(user);
                    await unitOfWork.SaveChangesAsync(default);
                }
            }
    
            await Clients.Others.SendAsync("UserIsOffline", new { UserId = userId, LastActive = DateTime.UtcNow });
        }
    
        await base.OnDisconnectedAsync(exception);
    }

    // Client will call this function to join into "chat room"
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    // Client will call this function when leaving the "chat room"
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
}