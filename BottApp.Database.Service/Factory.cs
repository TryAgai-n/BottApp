namespace BottApp.Database.Service;

public static class Factory
{
    public static IServiceContainer Create(IDatabaseContainer databaseContainer)
    {
        return new ServiceContainer(
            new DocumentService(databaseContainer.User, databaseContainer.Document), 
            new MessageService(databaseContainer.User, databaseContainer.Message));
    }
}