using BottApp.Database;
using BottApp.Host.Services.Handlers.AdminChat;
using BottApp.Host.Services.Handlers.Auth;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.Handlers.UploadHandler;
using BottApp.Host.Services.Handlers.Votes;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot.Services;

namespace BottApp.Host.Services.Handlers;

public static class Factory
{
    public static IHandlerContainer Create(IDatabaseContainer databaseContainer)
    {
        return new HandlerContainer(
            new AdminChatHandler(
                databaseContainer.User, 
                new MessageManager(databaseContainer.User, databaseContainer.Message)), 
            
            new AuthHandler(databaseContainer.User),
            
            new MainMenuHandler(
                databaseContainer.User, 
                new DocumentManager(databaseContainer.User, databaseContainer.Document),
                new StateStart(databaseContainer.User)
            ),
            
            new VotesHandler(
                databaseContainer.User,
                databaseContainer.Document,
                new DocumentManager(databaseContainer.User, databaseContainer.Document),
                new MessageManager(databaseContainer.User, databaseContainer.Message),
                new StateStart(databaseContainer.User)
            ),
            
            new CandidateUploadHandler(
                databaseContainer.User,
                new DocumentManager(databaseContainer.User, databaseContainer.Document)
            )
        );
    }
}