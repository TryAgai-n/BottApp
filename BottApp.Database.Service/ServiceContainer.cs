namespace BottApp.Database.Service;

public class ServiceContainer : IServiceContainer
{
    public IDocumentService Document { get; }
    public IMessageService Message { get; }

    public ServiceContainer(IDocumentService document, IMessageService message)
    {
        Document = document;
        Message = message;
    }
}