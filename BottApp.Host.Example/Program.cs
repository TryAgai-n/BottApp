using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Database;
using BottApp.Host.Example.Configs;
using BottApp.Host.Example.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace BottApp.Host.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var botConfig = builder.Configuration.GetSection("Bot").Get<BotConfig>();
            
       
            builder.Services.AddHttpClient("tgwebhook")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.Token, httpClient));

            
            var app = builder.Build();

            // app.UseEndpoints(endpoints =>
            // {
            //     var token = botConfig.Token;
            //     endpoints.MapControllerRoute(name: "tgwebhook",
            //         pattern: $"bot/{token}",
            //         new { controller = "Webhook", action = "Post" });
            //     endpoints.MapControllers();
            // });

            app.Run();

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
}