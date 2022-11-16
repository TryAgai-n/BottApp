using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.User;

public class UserRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateUserTest()
    {
        var user = DatabaseContainer.User.CreateUser(123, "Hello", null, UserState.Auth.ToString()).Result;
        Assert.NotNull(user);
        Assert.Equal(123, user.UId);
        Assert.Equal("Hello", user.FirstName);
        Assert.Null(user.Phone);
      

        var userByUid = DatabaseContainer.User.GetOneByUid(user.UId).Result;
        Assert.NotNull(userByUid);
        Assert.Equal(123, userByUid.UId);
    }


    [Fact]
    public void GetSetStateForUserTest()
    {
        var user = DatabaseContainer.User.CreateUser(123, "Hello", null, UserState.Auth.ToString()).Result;
          
        Assert.Equal("Auth", user.UserState);
        DatabaseContainer.User.SetState(user, UserState.Menu.ToString());
        Assert.Equal("Menu", user.UserState);
        var state = DatabaseContainer.User.GetStateByUid(123).Result;
        Assert.Equal("Menu", state);
    }
    // [Fact]
    // public void UpdateUserPhoneTest()
    // {
    //     var user = DatabaseContainer.User.CreateUser(3435, "Hello", null, UserState.Auth).Result;
    //     Assert.NotNull(user);
    //     Assert.Null(user.Phone);


    // var updatedUser = DatabaseContainer.User.UpdateUserPhone(user, "500").Result;
    //
    // Assert.NotNull(user.Phone);
    //     // Assert.Equal("500", user.Phone);
    // }
}