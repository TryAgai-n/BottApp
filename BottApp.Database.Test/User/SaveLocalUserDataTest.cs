using BottApp.Database.Document;
using BottApp.Database.User;
using Xunit;

namespace BottApp.Database.Test.User;

public class SaveLocalUserDataTest: DbTestCase
{
    
    private readonly List<LocalUser> _users = new ();

    [Fact]
    public void Add_And_RemoveUser()
    {
        _users.Add(new LocalUser(99));
        _users.Add(new LocalUser(101));

        var user = _users.FirstOrDefault(x => x.Id == 99);
        _users.Remove(user);
        
        Assert.Equal(1, _users.Count);
    }


    [Fact]
    public void Change_User_Flags_Test()
    {
        _users.Add(new LocalUser(1));
        _users.Add(new LocalUser(3));

        var user = _users?
            .FirstOrDefault(x => x.Id == 1);
        user.IsSendDocument = true;
        Assert.True(user.IsSendDocument);
        
        var user2 = _users?
            .FirstOrDefault(x => x.Id == 3);
        Assert.False(user2.IsSendDocument);
        
        var allUsers = _users?.Where(x=> x.Id < _users.Count).ToList();
        
        Assert.Equal(2, allUsers?.Count);
    }
}