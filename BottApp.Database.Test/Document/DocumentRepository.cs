using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.Document;

public class DocumentRepository: DbTestCase
{

    [Fact]
    public void CreateDocumentTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        


        Assert.NotNull(user);
   
        
        var document = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa").Result;
        var document2 = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa").Result;
        
        Assert.NotNull(document);
        Assert.NotNull(document2);
       // Assert.Equal(user.Id, document.UserId);
      //  Assert.Equal(document.documentType, "Photo");

        Assert.Equal(user.UId, document.UserModel.UId);
        Assert.NotNull(document.DocumentStatisticModel);
        
        Assert.Equal(0, document.DocumentStatisticModel.LikeCount);
    }
}