using BottApp.Database.User;
using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.Message;

public class MessageRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateMessageTest()
    {
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        
         var message = DatabaseContainer.Message.CreateModel(user.Id, "Message!!!",  "Voice", DateTime.Now).Result;
        
        
        Assert.Equal(user.Id, message.UserId);
    }
}