using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Host.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HandlerFactory = BottApp.Host.Handlers.Factory;
using ServiceFactory = BottApp.Database.Service.Factory;

namespace BottApp.Host
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment{ get; }

        
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            CurrentEnvironment = hostEnvironment;
        }
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

            
            services.AddSwaggerGen(
                c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "BottApp.Host.Exp", Version = "v1"}); }
            );
        }

        private void ConfigureCoreServices(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddScoped<IDatabaseContainer, DatabaseContainer>();

            services.AddScoped<IHandlerContainer>(x =>  HandlerFactory.Create(
                x.GetRequiredService<IDatabaseContainer>(),
                x.GetRequiredService<IServiceContainer>()
            ));


            services.AddScoped<IServiceContainer>(
                x => ServiceFactory.Create(x.GetRequiredService<IDatabaseContainer>()));

            TelegramBotStartup.ConfigureServices(services, Configuration);
            
            var typeOfContent = typeof(Startup);

            services.AddDbContextFactory<PostgreSqlContext>(
                opt => opt.UseNpgsql(
                    Configuration.GetConnectionString("PostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeOfContent.Assembly.GetName().Name)
                ),
                ServiceLifetime.Transient);

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BottApp.Host.Exp v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        
    }
}