using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using BottApp.Database.User.UserFlag;
using BottApp.Database.UserMessage;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserRepository User { get; }
        public IUserFlagRepository UserFlag { get;}
        public IMessageRepository Message { get; }
        
        public IDocumentRepository Document { get; }

        public IDocumentStatisticRepository DocumentStatistic { get; }

        public ILikedDocumentRepository LikeDocument { get; }
    


        public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
        {
            User = new UserRepository(db, loggerFactory);
            UserFlag = new UserFlagRepository(db, loggerFactory);
            Message = new MessageRepository(db, loggerFactory);
            Document = new DocumentRepository(db, loggerFactory);
            DocumentStatistic = new DocumentStatisticRepository(db, loggerFactory);
            LikeDocument = new LikedDocumentRepository(db, loggerFactory);
        }


    }
    
}
