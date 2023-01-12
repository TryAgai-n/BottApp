using BottApp.Host.Handlers.AdminChat;
using BottApp.Host.Handlers.Auth;
using BottApp.Host.Handlers.Help;
using BottApp.Host.Handlers.MainMenu;
using BottApp.Host.Handlers.UploadHandler;
using BottApp.Host.Handlers.Votes;

namespace BottApp.Host.Handlers;

public interface IHandlerContainer
{
    IAdminChatHandler AdminChatHandler { get; }
    IAuthHandler AuthHandler { get; }
    IMainMenuHandler MainMenuHandler { get; }
    IVotesHandler VotesHandler { get; }
    ICandidateUploadHandler CandidateUploadHandler { get; }
    IHelpHandler HelpHandler { get; }
}