using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Host.Services.Handlers.AdminChat;
using BottApp.Host.Services.Handlers.Auth;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.Handlers.UploadHandler;
using BottApp.Host.Services.Handlers.Votes;
using BottApp.Host.Services.OnStateStart;

namespace BottApp.Host.Services.Handlers;

public static class Factory
{
    public static IHandlerContainer Create(IDatabaseContainer databaseContainer, IServiceContainer serviceContainer)
    {
        return new HandlerContainer(
            new AdminChatHandler(
                databaseContainer.User, 
                serviceContainer.Message,
                serviceContainer.Document,
                databaseContainer.Document), 
            
            new AuthHandler(
                databaseContainer.User,
                serviceContainer.Message), 
            
            new MainMenuHandler(
                databaseContainer.User, 
                serviceContainer.Document,
                serviceContainer.Message,
                new StateService(databaseContainer.User, serviceContainer.Message)
            ),
            
            new VotesHandler(
                databaseContainer.User,
                databaseContainer.Document,
                databaseContainer.LikeDocument,
                serviceContainer.Document,
                serviceContainer.Message, 
                new StateService(databaseContainer.User, serviceContainer.Message)
            ),
            
            new CandidateUploadHandler(
                databaseContainer.User,
                databaseContainer.Document,
                serviceContainer.Document,
                new StateService(databaseContainer.User, serviceContainer.Message),
                serviceContainer.Message
            )
        );
    }
}