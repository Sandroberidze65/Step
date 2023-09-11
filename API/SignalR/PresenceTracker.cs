namespace API.SignalR;

public class PresenceTracker{

    private static readonly Dictionary<string, List<string>> _onlineUsers = new Dictionary<string, List<string>>();

    public Task<bool> UserConnected(string username, string connectionId){
        bool isOnline =false;
        lock(_onlineUsers){
            if(_onlineUsers.ContainsKey(username)){
                _onlineUsers[username].Add(connectionId);
            }
            else{
                _onlineUsers.Add(username, new List<string>{connectionId});
                isOnline = true;
            }
        }
        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconected(string username, string connectionId){
        bool isOfline = false;
        lock(_onlineUsers){
            if(!_onlineUsers.ContainsKey(username)) return Task.FromResult(isOfline);

            _onlineUsers[username].Remove(connectionId);

            if (_onlineUsers[username].Count == 0){
                _onlineUsers.Remove(username);
                isOfline = true;
            }

            return Task.FromResult(isOfline);
        }
    }

    public Task<string[]> GetOnlineUsers(){
        string[] onlineUsers;
        lock(_onlineUsers){
            onlineUsers = _onlineUsers.OrderBy(k => k.Key).Select(k=>k.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionFroUser(string username){
        List<string> connectionIds;

        lock(_onlineUsers){
            connectionIds = _onlineUsers.GetValueOrDefault(username);
        }

        return Task.FromResult(connectionIds);
    }
}