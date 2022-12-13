using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using BottApp.Database.UserMessage;

namespace BottApp.Database
{
    public interface IDatabaseContainer
    {
        IUserRepository User { get; }
        
        IMessageRepository Message { get; }
        
        IDocumentRepository Document { get; }
        
        IDocumentStatisticRepository DocumentStatistic { get; }
        
        ILikedDocumentRepository LikeDocument { get; }
    }
}
