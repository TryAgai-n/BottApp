using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BottApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BottApp.Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment{ get; set; }
        
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            CurrentEnvironment = hostEnvironment;
            _logger = loggerFactory.CreateLogger<Startup>();
            _loggerFactory = loggerFactory;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors
            (
                options =>
                {
                    options.AddPolicy
                    (
                        "CorsPolicy",
                        policy =>
                            policy.WithOrigins(Configuration.GetSection("AllowedHosts").Value)
                                .WithMethods("POST", "GET", "DELETE", "OPTIONS")
                                .WithHeaders("*")
                    );
                    options.AddPolicy
                    (
                        "apiDocumentation",
                        policy =>
                            policy.WithOrigins("*")
                                .WithMethods("POST", "GET", "DELETE")
                                .WithHeaders("*")
                    );
                }
            );
            
            ConfigureCoreServices(services, CurrentEnvironment);
            
        }

        private void ConfigureCoreServices(IServiceCollection services, IWebHostEnvironment env)
        {

            Type typeOfContent = typeof(Startup);
            
            services.AddDbContext<PostgreSqlContext>(
                opt => opt.UseNpgsql(
                    Configuration.GetConnectionString("PostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)
                )
            );

            services.AddScoped<IDatabaseContainer, DatabaseContainer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}


// Start Bot
// var token = builder.Configuration.GetSection("Token").Value;
// var bot = new BotInit();
//
// bot.initReceiver(token);
//
// var app = builder.Build();