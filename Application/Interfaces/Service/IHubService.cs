namespace Application.Interfaces.Service;

public interface IHubService
{
    Task SendMessageToGroupAsync(string conversationId, object messageContent);
}