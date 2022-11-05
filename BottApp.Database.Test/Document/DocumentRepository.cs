using Xunit;

namespace BottApp.Database.Test.Document;

public class DocumentRepository: DbTestCase
{

    [Fact]
    public void CreateDocumentTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        


        Assert.NotNull(user);
   
        
        var document = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa");
        
        Assert.NotNull(document);
       // Assert.Equal(user.Id, document.UserId);
      //  Assert.Equal(document.documentType, "Photo");

        Assert.Equal(3435, user.UId);
    }
}