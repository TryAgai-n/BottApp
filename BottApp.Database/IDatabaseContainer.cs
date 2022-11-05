using BottApp.Database.Message;
using BottApp.Database.Document;
using BottApp.Database.User;

namespace BottApp.Database
{
    public interface IDatabaseContainer
    {
        IUserRepository User { get; }
        
        IMessageRepository Message { get; }
        
        IDocumentRepository Document { get; }
    }
}
