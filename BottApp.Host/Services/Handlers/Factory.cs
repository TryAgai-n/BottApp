using BottApp.Database;
using BottApp.Host.Services.Handlers.AdminChat;
using BottApp.Host.Services.Handlers.Auth;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.Handlers.Votes;

namespace BottApp.Host.Services.Handlers;

public static class Factory
{
    public static IHandlerContainer Create(IDatabaseContainer databaseContainer)
    {
        return new HandlerContainer(
            new AdminChatHandler(databaseContainer.User),
            new AuthHandler(),
            new MainMenuHandler(databaseContainer.User, new DocumentManager(databaseContainer.User, databaseContainer.Document)),
            new VotesHandler()
            
        );
    }
}