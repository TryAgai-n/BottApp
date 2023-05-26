
namespace BottApp.Host;

static internal class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
        
    private static IHostBuilder CreateHostBuilder(string[] args) =>  Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}