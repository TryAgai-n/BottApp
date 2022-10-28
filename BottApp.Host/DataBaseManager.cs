using BottApp.Database;
using BottApp.Database.User;

namespace BottApp.Host;

public class DataBaseManager
{
    private readonly IDatabaseContainer _databaseContainer;

    public DataBaseManager(IDatabaseContainer databaseContainer)
    {
        _databaseContainer = databaseContainer;
    }
    public async Task<UserModel> Test(int uid, string firstName, string userPhone, bool isSendContact)
    {
        var user = await _databaseContainer.User.CreateUser(uid, firstName,userPhone, isSendContact);
        return user;
    }
}



