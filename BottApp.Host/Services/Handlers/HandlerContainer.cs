using BottApp.Host.Services.Handlers.AdminChat;
using BottApp.Host.Services.Handlers.Auth;
using BottApp.Host.Services.Handlers.MainMenu;
using BottApp.Host.Services.Handlers.Votes;

namespace BottApp.Host.Services.Handlers;

public class HandlerContainer : IHandlerContainer
{
    public IAdminChatHandler AdminChatHandler { get; }
    public IAuthHandler AuthHandler { get; }
    public IMainMenuHandler MainMenuHandler { get; }
    public IVotesHandler VotesHandler { get; }


    public HandlerContainer(
        IAdminChatHandler adminChatHandler,
        IAuthHandler authHandler,
        IMainMenuHandler mainMenuHandler,
        IVotesHandler votesHandler
    )
    {
        AdminChatHandler = adminChatHandler;
        AuthHandler = authHandler;
        MainMenuHandler = mainMenuHandler;
        VotesHandler = votesHandler;
    }
}