using System.Collections.Concurrent;

namespace DirassatiBackend.Common.Services.ConnectionTracker
{
    public class ConnectionTracker : IConnectionTracker
    {
        private readonly ConcurrentDictionary<string, List<string>> _userConnections = new();

        public void AddConnection(string userId, string connectionId)
        {
            _userConnections.AddOrUpdate(userId,
                new List<string> { connectionId },
                (key, existingList) =>
                {
                    existingList.Add(connectionId);
                    return existingList;
                });
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            _userConnections.AddOrUpdate(userId,
                new List<string>(),
                (key, existingList) =>
                {
                    existingList.Remove(connectionId);
                    return existingList;
                });

            if (_userConnections.TryGetValue(userId, out var connections) && !connections.Any())
            {
                _userConnections.TryRemove(userId, out _);
            }
        }

        public bool IsUserOnline(string userId)
        {
            return _userConnections.ContainsKey(userId) && _userConnections[userId].Any();
        }
    }
}