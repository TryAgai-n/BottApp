using BottApp.Database;

namespace BottApp.Host;

public class MyHostedService : IHostedService
{
    private readonly IServiceScopeFactory scopeFactory;

    public MyHostedService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public void DoWork()
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}