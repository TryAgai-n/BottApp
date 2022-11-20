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
        var documentInBasePath1 = DatabaseContainer.Document.CreateModel(user.Id, "documentInBasePath1", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Base).Result;
        var documentInBasePath2 = DatabaseContainer.Document.CreateModel(user.Id, "documentInBasePath2", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Base).Result;
        var documentInBasePath3 = DatabaseContainer.Document.CreateModel(user.Id, "documentInBasePath3", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Base).Result;
        var documentInVotesPath1 = DatabaseContainer.Document.CreateModel(user.Id, "documentInVotesPath1", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Votes).Result;
        var documentInVotesPath2 = DatabaseContainer.Document.CreateModel(user.Id, "documentInVotesPath2", ".jpeg", DateTime.Now, "//path//saqwe//fsa", DocumentInPath.Votes).Result;


        var listMostViewedDocuments = DatabaseContainer.Document.ListMostViewedDocuments(0, 10).Result;
        
        Assert.Equal(5, listMostViewedDocuments.Count);

        var listInVotes = DatabaseContainer.Document.ListDocumentsByPath(pagination, DocumentInPath.Votes).Result;
        Assert.Equal(2, listInVotes.Count);

        var listInBase = DatabaseContainer.Document.ListDocumentsByPath(pagination, DocumentInPath.Base).Result;
        Assert.Equal(3, listInBase.Count);

        var skipList = DatabaseContainer.Document.ListDocumentsByPath(new Pagination(2, 1), DocumentInPath.Base).Result;
        Assert.Single(skipList);
        Assert.Equal(documentInBasePath3.DocumentType, skipList.First().DocumentType);

    }
}