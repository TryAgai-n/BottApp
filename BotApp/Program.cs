using Microsoft.AspNetCore;

namespace BottApp.Host.Exp;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("ddd");
        BuildWebHost(args);
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
}

