using BottApp.Database.Message;
using BottApp.Database.Document;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;

namespace BottApp.Database
{
    public interface IDatabaseContainer
    {
        IUserBotRepository UserBot { get; }
        
        IUserWebRepository UserWeb { get; }
        
        IMessageRepository Message { get; }
        
        IDocumentRepository Document { get; }
        
        IDocumentStatisticRepository DocumentStatistic { get; }
    }
}
