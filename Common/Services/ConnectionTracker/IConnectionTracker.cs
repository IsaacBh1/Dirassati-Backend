namespace DirassatiBackend.Common.Services.ConnectionTracker
{
    public interface IConnectionTracker
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string userId, string connectionId);
        bool IsUserOnline(string userId);
    }
}