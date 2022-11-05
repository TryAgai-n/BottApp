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
}