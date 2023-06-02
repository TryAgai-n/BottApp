

using VoteApp.Database.Document;
using VoteApp.Database.Document.Statistic;
using VoteApp.Database.User;

namespace VoteApp.Database
{
    public interface IDatabaseContainer
    {

        IUserWebRepository UserWeb { get; }
        
        
        IDocumentRepository Document { get; }
        
        IDocumentStatisticRepository DocumentStatistic { get; }
    }
}
