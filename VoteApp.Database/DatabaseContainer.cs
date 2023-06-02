using Microsoft.Extensions.Logging;
using VoteApp.Database.Document;
using VoteApp.Database.Document.Statistic;
using VoteApp.Database.User;

namespace VoteApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserWebRepository UserWeb { get; set; }

        public IDocumentRepository Document { get; }

        public IDocumentStatisticRepository DocumentStatistic { get; }


        public DatabaseContainer(PostgresContext db, ILoggerFactory loggerFactory)
        {
            UserWeb = new UserWebRepository(db, loggerFactory);
            Document = new DocumentRepository(db, loggerFactory);
            DocumentStatistic = new DocumentStatisticRepository(db, loggerFactory);
        }


    }
    
}
