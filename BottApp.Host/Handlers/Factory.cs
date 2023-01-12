using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Host.Handlers.AdminChat;
using BottApp.Host.Handlers.Auth;
using BottApp.Host.Handlers.Help;
using BottApp.Host.Handlers.MainMenu;
using BottApp.Host.Handlers.UploadHandler;
using BottApp.Host.Handlers.Votes;
using BottApp.Host.Services;

namespace BottApp.Host.Handlers;

public static class Factory
{
    public static IHandlerContainer Create(
        IDatabaseContainer databaseContainer,
        IServiceContainer serviceContainer
    )
    {
        return new HandlerContainer(
            new AdminChatHandler(
                databaseContainer.User, 
                serviceContainer.Message,
                serviceContainer.Document,
                databaseContainer.Document), 
            
            new AuthHandler(
                databaseContainer.User,
                serviceContainer.Message
                ), 
            
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
               // serviceContainer.Message, 
                new StateService(databaseContainer.User, serviceContainer.Message)
                ),
            
            new CandidateUploadHandler(
                databaseContainer.User,
                databaseContainer.Document,
                serviceContainer.Document,
                new StateService(databaseContainer.User, serviceContainer.Message),
                serviceContainer.Message
                ),
            
            new HelpHandler(
                databaseContainer.User, 
                serviceContainer.Document,
                serviceContainer.Message,
                new StateService(databaseContainer.User, serviceContainer.Message)
                )
            
            
        );
    }
}