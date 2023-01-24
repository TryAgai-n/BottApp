using BottApp.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BottApp.Database.Test;

internal sealed class Fixture : IDisposable
{
    private PostgreSqlContext _postgreSqlContext;
    public DatabaseContainer DatabaseContainer;
    public IServiceScopeFactory scopeFactory;

    public Fixture(PostgreSqlContext postgreSqlContext, IServiceScopeFactory scopeFactory)
    {
        _postgreSqlContext = postgreSqlContext;
        DatabaseContainer = postgreSqlContext.Db;
        this.scopeFactory = scopeFactory;
    }


    public Fixture Create()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    
        var guid = Guid.NewGuid().ToString("N");
        var option = new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseNpgsql(
                "User ID=postgres;Password=123;Host=localhost;Port=5432;Database=bottapp_test_" + guid + ";Pooling=true;",
                b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name)
                )
            .Options;
    
        var context = new PostgreSqlContext(option, new NullLoggerFactory(), scopeFactory);
    
        context.Database.Migrate();
        
        return new Fixture(context, scopeFactory);
    }

    public void Dispose()
    {
        if (_postgreSqlContext == null)
        {
            return;
        }
        
        _postgreSqlContext.Database.EnsureDeleted();
        _postgreSqlContext?.Dispose();
    }
}