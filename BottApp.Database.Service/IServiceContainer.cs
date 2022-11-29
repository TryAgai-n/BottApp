namespace BottApp.Database.Service;

public interface IServiceContainer
{
   IDocumentService Document { get; } 
   IMessageService Message { get; }
}