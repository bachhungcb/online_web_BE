using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class ChatHub : Hub
{
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