using BottApp.Database.Document;
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
   
        
        var document = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Base).Result;
        var document2 = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Votes).Result;
        
        Assert.NotNull(document);
        Assert.NotNull(document2);
       // Assert.Equal(user.Id, document.UserId);
      //  Assert.Equal(document.documentType, "Photo");

        Assert.Equal(user.UId, document.UserModel.UId);
        Assert.NotNull(document.DocumentStatisticModel);
        
        Assert.Equal(0, document.DocumentStatisticModel.LikeCount);


        var getDocumetById = DatabaseContainer.Document.GetOneByDocumentId(document.Id).Result;
        Assert.NotNull(getDocumetById);
        Assert.Equal(document.Path, getDocumetById.Path);
    }



    [Fact]
    public void ListDocumentsTest()
    {
        var user = DatabaseContainer.User.CreateUser(3435, "Hello", null).Result;
        Assert.NotNull(user);

        var pagination = new Pagination(0, 10);
        var basePathCollection = new List<DocumentModel>();
        var votesPathCollection = new List<DocumentModel>();

        for (var i = 1; i <= 10; i++)
        {
            basePathCollection.Add(DatabaseContainer.Document.CreateModel(user.Id, $"Base Type {i}", ".jpeg", DateTime.Now, "Base Path", DocumentInPath.Base).Result);
        }
        for (var i = 1; i <= 5; i++)
        {
            votesPathCollection.Add(DatabaseContainer.Document.CreateModel(user.Id, $"Votes Type {i}", ".jpeg", DateTime.Now, "Vote Path", DocumentInPath.Votes).Result);
        }

        Assert.Equal(10, basePathCollection.Count);
        // Assert.Equal(5, votesPathCollection.Count);

        var listInBase = DatabaseContainer.Document.ListDocumentsByPath(pagination, DocumentInPath.Base).Result;
        Assert.Equal(10, listInBase.Count);


        var firstDocumentInVotes = DatabaseContainer.Document.GetFirstDocumentByPath(DocumentInPath.Votes).Result;

        Assert.Equal("Votes Type 1", firstDocumentInVotes.DocumentType);

    }
}