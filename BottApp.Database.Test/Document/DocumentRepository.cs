using BottApp.Database.Document;
using BottApp.Database.User;
using BottApp.Host.SimpleStateMachine;
using Xunit;

namespace BottApp.Database.Test.Document;

public class DocumentRepository: DbTestCase
{
    [Fact]
    public void CreateDocumentTest()
    {
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        Assert.NotNull(user);
   
        
        var document = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", "Описание",DocumentInPath.Votes, InNomination.ㅤ).Result;
        var document2 = DatabaseContainer.Document.CreateModel(user.Id, "Photo", ".jpeg", DateTime.Now, "//path//saqwe//fsa", "Описание",DocumentInPath.Votes, InNomination.ㅤㅤ).Result;

        Assert.NotNull(document);
        Assert.NotNull(document2);
        Assert.Equal(user.Id, document.UserId);
        Assert.Equal("Photo", document.DocumentType);

        Assert.Equal(user.UId, document.UserModel.UId);
        Assert.NotNull(document.DocumentStatisticModel);

        Assert.Equal(0, document.DocumentStatisticModel.LikeCount);


        var documentModel = DatabaseContainer.Document.GetOneByDocumentId(document.Id).Result;
        Assert.NotNull(documentModel);
        Assert.Equal(document.Path, documentModel.Path);

        var incrementViewCount =  DatabaseContainer.Document.IncrementViewByDocument(documentModel);
        incrementViewCount =  DatabaseContainer.Document.IncrementViewByDocument(documentModel);
        // document = DatabaseContainer.Document.GetOneByDocumentId(document.Id).Result;
        
        
        Assert.Equal(2, documentModel.DocumentStatisticModel.ViewCount);

    }



    [Fact]
    public void ListDocumentsTest()
    {
        var telegramProfile = new TelegramProfile(3435, "FirstName", "LastName", null);
        var user = DatabaseContainer.User.CreateUser(telegramProfile).Result;
        Assert.NotNull(user);
        
        // var basePathCollection = new List<DocumentModel>();
        var votesPathCollection = new List<DocumentModel>();
        //
        // for (var i = 1; i <= 20; i++)
        // {
        //     basePathCollection.Add(DatabaseContainer.Document.CreateModel(user.Id, $"Base Type {i}", ".jpeg", DateTime.Now, "Base Path", DocumentInPath.Base, null).Result);
        // }
        for (var i = 1; i <= 3; i++)
        {
            votesPathCollection.Add(DatabaseContainer.Document.CreateModel(
                user.Id,
                $"Votes Type {i}",
                ".jpeg",
                DateTime.Now,
                "Vote Path",
                "Описание",
                DocumentInPath.Votes,
                InNomination.ㅤ).Result);
        }
        
        
        for (var i = 1; i <= 6; i++)
        {
            votesPathCollection.Add(DatabaseContainer.Document.CreateModel(
                user.Id,
                $"Votes Type {i}",
                ".jpeg",
                DateTime.Now,
                "Vote Path",
                "Описание",
                DocumentInPath.Votes,
                InNomination.ㅤㅤ).Result);
        }
        

        // Assert.Equal(20, basePathCollection.Count);
        Assert.Equal(9, votesPathCollection.Count);
        
        Assert.Equal(InNomination.ㅤ, votesPathCollection[1].DocumentNomination);
        
        var CountInBiggest = DatabaseContainer.Document.ListDocumentsByNomination(InNomination.ㅤ, 0).Result;
        var CountInFast = DatabaseContainer.Document.ListDocumentsByNomination(InNomination.ㅤㅤ, 0).Result;
        
        Assert.Equal(3, CountInBiggest.Count);
        Assert.Equal(6, CountInFast.Count);

        var listInBase = DatabaseContainer.Document.ListDocumentsByPath(DocumentInPath.Base).Result;
        Assert.Equal(1000, listInBase.Count);

        
        var firstDocumentInVotes = DatabaseContainer.Document.GetFirstDocumentByPath(DocumentInPath.Votes).Result;
        
        Assert.Equal("Votes Type 1", firstDocumentInVotes.DocumentType);
        
        var documentCollection = DatabaseContainer.Document.ListDocumentsByPath(DocumentInPath.Votes).Result;
        
        
        Assert.Equal(2, documentCollection.Count);
    }
}