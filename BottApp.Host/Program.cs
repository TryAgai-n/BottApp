using BottApp.Database;
using BottApp.Host;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;

public class Program
{
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        var webHost = BuildWebHost(args);
        var commandLineApplication = new CommandLineApplication(false);

        var catapult = commandLineApplication.Command(
            "command",
            config =>
            {
                config.OnExecute(
                    () =>
                    {
                        config.ShowHelp();
                        return 1;
                    }
                );
                config.HelpOption("-? | -h | --help");
            }
        );

        var doMigrate = commandLineApplication.Option(
            "--ef-migrate",
            "Apply entity framework migrations and exit",
            CommandOptionType.NoValue
        );
        var verifyMigrate = commandLineApplication.Option(
            "--ef-migrate-check",
            "Check the status of entity framework migrations",
            CommandOptionType.NoValue
        );
        var run = commandLineApplication.Option(
            "--run",
            "Run api Server",
            CommandOptionType.NoValue
        );

        commandLineApplication.HelpOption("-? | -h | --help");
        commandLineApplication.OnExecute(
            () =>
            {
                ExecuteApp(webHost, doMigrate, verifyMigrate, run);
                return 0;
            }
        );
        commandLineApplication.Execute(args);
    }


    public static IWebHost BuildWebHost(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseUrls(config.GetSection("Url").Value)
            .Build();
    }


    private static IWebHost ExecuteApp(
        IWebHost webHost,
        CommandOption doMigrate,
        CommandOption verifyMigrate,
        CommandOption run
    )
    {
        if (run.HasValue())
        {
            InitWebServices(webHost).GetAwaiter().GetResult();
            return webHost;
        }

        if (verifyMigrate.HasValue() && doMigrate.HasValue())
        {
            Console.WriteLine("ef-migrate and ef-migrate-check are mutually exclusive, select one, and try again");
            Environment.Exit(2);
        }

        var serviceScopeFactory = (IServiceScopeFactory) webHost.Services.GetService(typeof(IServiceScopeFactory))!;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var services = scope.ServiceProvider;
            using (var context = services.GetRequiredService<PostgreSqlContext>())
            {
                if (verifyMigrate.HasValue())
                {
                    Console.WriteLine("Validating status of Entity Framework migrations");
                    _verifyMigrate(context);
                    Environment.Exit(3);
                }

                if (doMigrate.HasValue())
                {
                    Console.WriteLine("Applyting Entity Framework migrations");
                    context.Database.Migrate();
                    Console.WriteLine("All done, closing app");
                    Environment.Exit(0);
                }
            }
        }

        InitWebServices(webHost).GetAwaiter().GetResult();
        return webHost;
    }


    private static void _verifyMigrate(PostgreSqlContext context)
    {
        var pendingMigrations = context.Database.GetPendingMigrations();
        var migrations = pendingMigrations as IList<string> ?? pendingMigrations.ToList();
        if (!migrations.Any())
        {
            Console.WriteLine("No pending migratons");
            Environment.Exit(0);
        }

        Console.WriteLine("Pending migratons {0}", migrations.Count());
        foreach (var migration in migrations)
        {
            Console.WriteLine($"\t{migration}");
        }
    }


    private static async Task<int> InitWebServices(IWebHost webHost)
    {
        await Task.WhenAll(webHost.RunAsync());

        await Task.WhenAll(webHost.StopAsync());
        Environment.Exit(0);
        return 0;
    }
}