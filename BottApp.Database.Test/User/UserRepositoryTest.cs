using Xunit;

namespace BottApp.Database.Test.User;

public class UserRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateUserTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        
        Assert.Equal(3435, user.UId);
    }

    [Fact]
    public void UpdateUserPhoneTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        Assert.NotNull(user);
        Assert.Null(user.Phone);


        var updatedUser = DatabaseContainer.User.UpdateUserPhone(user, "500").Result;
        
        Assert.NotNull(user.Phone);
        Assert.Equal("500", user.Phone);
    }
}