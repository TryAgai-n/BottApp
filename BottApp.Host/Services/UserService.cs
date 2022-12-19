using BottApp.Database.User;

namespace BottApp.Host.Services;

public static class UserService
{
    private static List<LocalUser?> _localUsers = new();
    public static async Task Add_User_To_List(int userId)
    {
        _localUsers.Add(new LocalUser(userId));
        var user = _localUsers.FirstOrDefault(x => x.Id == userId);
    }
    
    public static async Task Remove_User_InTo_List(int userId)
    { 
        var user = _localUsers.FirstOrDefault(x => x.Id == userId);
        _localUsers.Remove(user);
    }
    
    public static async Task<LocalUser?> Get_One_User(int userId)
    {
        return _localUsers
            .FirstOrDefault(x => x.Id == userId);
    }
}