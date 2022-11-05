using BottApp.Utils;
using Xunit;

namespace BottApp.Database.Test.Message;

public class MessageRepositoryTest : DbTestCase
{
    [Fact]
    public void CreateMessageTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;


        // var message = DatabaseContainer.Message.CreateModel(user.Id, "Message!!!").Result;
        
        
        // Assert.Equal(user.Id, message.UserId);
    }
}