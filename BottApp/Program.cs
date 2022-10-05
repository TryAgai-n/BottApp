using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace BottApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var webHost = BuildWebHost(args);
        await InitWebServices(webHost);
        
        
        // var token = GetConfig.GetBotToken();
        // var bot = new TelegramBotClient(token);
        // ProgramTemp.initReceiver(bot);
    }
    
    public static IWebHost BuildWebHost(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
//                .UseUrls(config.GetSection("Url").Value)
            .Build();
    }
    
    private static async Task<int> InitWebServices(IWebHost webHost)
    {
        await Task.WhenAll(
            webHost.RunAsync()
        );

        Console.Read();
        await Task.WhenAll(
            webHost.StopAsync()
        );
        Environment.Exit(0);
        return 0;
    }
}