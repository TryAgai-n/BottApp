using BottApp.Database.BotUser;
using BottApp.Database.Message;
using BottApp.Database.Document;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserBotRepository UserBot { get; }
        public IUserWebRepository UserWeb { get; set; }
        public IMessageRepository Message { get; }
        
        public IDocumentRepository Document { get; }

        public IDocumentStatisticRepository DocumentStatistic { get; }


        public DatabaseContainer(PostgresContext db, ILoggerFactory loggerFactory)
        {
            UserBot = new UserBotRepository(db, loggerFactory);
            UserWeb = new UserWebRepository(db, loggerFactory);
            Message = new MessageRepository(db, loggerFactory);
            Document = new DocumentRepository(db, loggerFactory);
            DocumentStatistic = new DocumentStatisticRepository(db, loggerFactory);
        }


    }
    
}
