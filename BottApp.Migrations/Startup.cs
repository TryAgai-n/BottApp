using BottApp.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BottApp.Migrations
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        
        public void ConfigureServices(IServiceCollection services)
        {
            var typeOfContent = typeof(Startup);

            services.AddDbContext<PostgresContext>(
                options => options.UseNpgsql(
                    Configuration.GetConnectionString("PostgresConnection"),
                    b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)
                )
            );
            
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        { }
    }
}