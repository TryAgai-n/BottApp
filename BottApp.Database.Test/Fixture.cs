using BottApp.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BottApp.Database.Test;

internal sealed class Fixture : IDisposable
{
    private PostgreSqlContext _postgreSqlContext;
    public DatabaseContainer DatabaseContainer;

    public Fixture(PostgreSqlContext postgreSqlContext)
    {
        _postgreSqlContext = postgreSqlContext;
        DatabaseContainer = postgreSqlContext.Db;
    }


    public static Fixture Create()
    {

        var guid = Guid.NewGuid().ToString("N");
        var option = new DbContextOptionsBuilder<PostgreSqlContext>()
            .UseNpgsql(
                "User ID=postgres;Password=123;Host=localhost;Port=5432;Database=bottapp_test_" + guid + ";Pooling=true;",
                b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name)
                )
            .Options;

        var context = new PostgreSqlContext(option, new NullLoggerFactory());

        context.Database.Migrate();
        
        return new Fixture(context);
    }

    public void Dispose()
    { 
        _postgreSqlContext.Dispose();
     //   _postgreSqlContext.Database.EnsureDeleted();
    }
}