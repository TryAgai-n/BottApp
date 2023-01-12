using BottApp.Host.Handlers.AdminChat;
using BottApp.Host.Handlers.Auth;
using BottApp.Host.Handlers.Help;
using BottApp.Host.Handlers.MainMenu;
using BottApp.Host.Handlers.UploadHandler;
using BottApp.Host.Handlers.Votes;

namespace BottApp.Host.Handlers;

public class HandlerContainer : IHandlerContainer
{
    public IAdminChatHandler AdminChatHandler { get; }
    public IAuthHandler AuthHandler { get; }
    public IMainMenuHandler MainMenuHandler { get; }
    public IVotesHandler VotesHandler { get; }
    public ICandidateUploadHandler CandidateUploadHandler { get; }
    
    public IHelpHandler HelpHandler { get; }


    public HandlerContainer(
        IAdminChatHandler adminChatHandler,
        IAuthHandler authHandler,
        IMainMenuHandler mainMenuHandler,
        IVotesHandler votesHandler,
        ICandidateUploadHandler candidateUploadHandler,
        IHelpHandler helpHandler)
    {
        AdminChatHandler = adminChatHandler;
        AuthHandler = authHandler;
        MainMenuHandler = mainMenuHandler;
        VotesHandler = votesHandler;
        CandidateUploadHandler = candidateUploadHandler;
        HelpHandler = helpHandler;
    }
}