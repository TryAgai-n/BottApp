using Xunit;

namespace BottApp.Database.Test.User;

public class UserRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateUserTest()
    {
        var user = DatabaseContainer.User.CreateUser(123, "Hello", null).Result;
        Assert.NotNull(user);
        Assert.Equal(123, user.UId);
        Assert.Equal("Hello", user.FirstName);
        Assert.Null(user.Phone);

        var userByUid = DatabaseContainer.User.GetOneByUid(user.UId).Result;
        Assert.NotNull(userByUid);
        Assert.Equal(123, userByUid.UId);
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