using Telegram.Bot.Services;

namespace BottApp.Host.Services.Handlers;

public abstract class AbstractUpdateHandler
{
    protected readonly IHandlerContainer _handlerContainer;
    


    protected AbstractUpdateHandler(IHandlerContainer handlerContainer)
    {
        _handlerContainer = handlerContainer;
    }
}