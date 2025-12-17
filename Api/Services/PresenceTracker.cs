namespace Api.Services;

public class PresenceTracker
{
    private static readonly Dictionary<Guid, int> OnlineUsers = new Dictionary<Guid, int>();

    public Task<bool> UserConnected(Guid userId)
    {
        bool isOnline = false;
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(userId))
            {
                OnlineUsers[userId]++;
            }
            else
            {
                OnlineUsers.Add(userId, 1);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(Guid userId)
    {
        bool isOffline = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(userId)) return Task.FromResult(isOffline);

            OnlineUsers[userId]--;
            if (OnlineUsers[userId] == 0)
            {
                OnlineUsers.Remove(userId);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<Guid[]> GetOnlineUsers()
    {
        Guid[] onlineList;
        lock (OnlineUsers)
        {
            onlineList = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }

        return Task.FromResult(onlineList);
    }
}