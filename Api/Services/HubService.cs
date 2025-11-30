using Api.Hubs;
using Application.Interfaces.Service;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public class HubService : IHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public HubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendMessageToGroupAsync(string conversationId, object messageContent)
    {
        // Gửi sự kiện "ReceiveMessage" đến tất cả client đang join group conversationId
        await _hubContext.Clients.Group(conversationId)
            .SendAsync("ReceiveMessage", messageContent);
    }
}