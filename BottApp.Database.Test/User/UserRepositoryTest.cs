using BottApp.Database.User;
using BottApp.Host.SimpleStateMachine;
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
        Assert.Equal("Hello", user.TelegramFirstName);
        Assert.Null(user.Phone);


        var userByUid = DatabaseContainer.User.GetOneByUid(user.UId).Result;
        Assert.NotNull(userByUid);
        Assert.Equal(123, userByUid.UId);


        var findUserByUid = DatabaseContainer.User.FindOneByUid(20).Result;
        if (findUserByUid == null)
        {
            findUserByUid = DatabaseContainer.User.CreateUser(20, "First", null).Result;
        }


        Assert.NotNull(findUserByUid);
        Assert.Equal(20, findUserByUid.UId);
        Assert.NotEqual(50, findUserByUid.UId);
        Assert.Equal("First", findUserByUid.TelegramFirstName);
        Assert.Null(findUserByUid.Phone);
        Assert.Equal(OnState.Auth, findUserByUid.OnState);





    }


    [Fact]
    public void ChangeUserOnStateTest()
    {
        var user = DatabaseContainer.User.CreateUser(123, "Hello", null).Result;
        Assert.Equal(OnState.Auth, user.OnState);

        var changeUserOnState = DatabaseContainer.User.ChangeOnState(user, OnState.Menu).Result;

        Assert.Equal(OnState.Menu, user.OnState);
    }


    [Fact]
    public void UpdateUserFullName()
    {
        var user = DatabaseContainer.User.CreateUser(011, "TgFirstName", null).Result;
        
        var updateFirstname = DatabaseContainer.User.UpdateUserFullName(user,"Имя", null).Result;
        
        Assert.NotNull(user);
        Assert.NotNull(user.FirstName);
        Assert.Null(user.LastName);
        
        _ = DatabaseContainer.User.UpdateUserFullName(user,"Имя", "Фамилия").Result;
        
        Assert.Equal("Имя", user.FirstName);
        Assert.Equal("Фамилия",user.LastName);

        
        _ = DatabaseContainer.User.UpdateUserFirstName(user,"Новое имя").Result;
        _ = DatabaseContainer.User.UpdateUserLastName(user,"Новая фамилия").Result;
        
        Assert.Equal("Новое имя", user.FirstName);
        Assert.Equal("Новая фамилия",user.LastName);
        
     //   _ = DatabaseContainer.User.UpdateUserFirstName(user,"Новое имя").Result;
     //  _ = DatabaseContainer.User.UpdateUserLastName(user,"Новая фамилия").Result;
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