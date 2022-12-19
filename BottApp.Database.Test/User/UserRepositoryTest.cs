using BottApp.Database.User;
using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.User;

public class UserRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateUserTest()
    {
        
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        
        
        Assert.Equal(3435, user.UId);
        Assert.Equal("FirstName", user.TelegramFirstName);
        Assert.Null(user.Phone);


        var userByUid = DatabaseContainer.User.GetOneByUid(user.UId).Result;
        Assert.NotNull(userByUid);
        Assert.Equal(3435, userByUid.UId);
        userByUid.ViewDocumentID = 1;



        // var updatedUserPhone = DatabaseContainer.User.UpdateUserPhone(user, "12345").Result;
        user.Phone = "54321";

        var findUser = DatabaseContainer.User.FindOneByUid(3435).Result;
        Assert.NotNull(findUser.Phone);
        Assert.Equal("54321", findUser.Phone);


     
        
        Assert.Equal(1, findUser.ViewDocumentID);




    }
    
    [Fact]
    public void ChangeUserOnStateTest()
    {
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        
        Assert.Equal(OnState.Auth, user.OnState);

        var changeUserOnState = DatabaseContainer.User.ChangeOnState(user, OnState.Menu).Result;

        Assert.Equal(OnState.Menu, user.OnState);
    }
    
    [Fact]
    public void UpdateUserProfileTest()
    {
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        Assert.NotNull(user);

        var profile = new Profile("Name", "Last");
        var updateProfile = DatabaseContainer.User.UpdateUserFullName(user, profile).Result;
        
        Assert.Equal(profile.FirstName, user.FirstName);
        Assert.Equal(profile.LastName,user.LastName);
        

        
        // _ = DatabaseContainer.User.UpdateUserFullName(user,"Новое имя", null).Result;
        // _ = DatabaseContainer.User.UpdateUserFullName(user, null,"Новая фамилия").Result;
        //
        // Assert.Equal("Новое имя", user.FirstName);
        // Assert.Equal("Новая фамилия",user.LastName);
        //
        // Assert.Equal("Новая фамилия" + ", " + "Новое имя",user.FullName);
        
     //   _ = DatabaseContainer.User.UpdateUserFirstName(user,"Новое имя").Result;
     //  _ = DatabaseContainer.User.UpdateUserLastName(user,"Новая фамилия").Result;
    }

}