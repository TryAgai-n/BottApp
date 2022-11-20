using BottApp.Database.Message;
using BottApp.Database.Document;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserRepository User { get; }

        public IMessageRepository Message { get; }
        
        public IDocumentRepository Document { get; }

        public IDocumentStatisticRepository DocumentStatistic { get; }


        public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
        {
            User = new UserRepository(db, loggerFactory);
            Message = new MessageRepository(db, loggerFactory);
            Document = new DocumentRepository(db, loggerFactory);
            DocumentStatistic = new DocumentStatisticRepository(db, loggerFactory);
        }


    }
    
}
