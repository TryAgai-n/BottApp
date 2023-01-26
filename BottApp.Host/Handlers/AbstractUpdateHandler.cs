
namespace BottApp.Host.Handlers;

public abstract class AbstractUpdateHandler
{
    protected readonly IHandlerContainer _handlerContainer;

    protected AbstractUpdateHandler(IHandlerContainer handlerContainer)
    {
        _handlerContainer = handlerContainer;
    }
}