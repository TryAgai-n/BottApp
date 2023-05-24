using BottApp.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BottApp.Database.Test;

internal sealed class Fixture : IDisposable
{
    private PostgresContext _postgresContext;
    public DatabaseContainer DatabaseContainer;

    public Fixture(PostgresContext postgresContext)
    {
        _postgresContext = postgresContext;
        DatabaseContainer = postgresContext.Db;
    }


    public static Fixture Create()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var guid = Guid.NewGuid().ToString("N");
        var option = new DbContextOptionsBuilder<PostgresContext>()
            .UseNpgsql(
                "User ID=postgres;Password=123;Host=localhost;Port=5432;Database=bottapp_test_" + guid + ";Pooling=true;",
                b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name)
                )
            .Options;

        var context = new PostgresContext(option, new NullLoggerFactory());

        context.Database.Migrate();
        
        return new Fixture(context);
    }

    public void Dispose()
    {
        if (_postgresContext == null)
        {
            return;
        }
        
        _postgresContext.Database.EnsureDeleted();

        _postgresContext?.Dispose();
    }
}