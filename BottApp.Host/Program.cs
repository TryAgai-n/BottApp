using BottApp.Data;
using BottApp.Host;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PostgreSqlContext>(
    o=> o.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));
builder.Services.AddScoped<IDatabaseContainer, DatabaseContainer>();

//Start Bot
var token = builder.Configuration.GetSection("Token").Value;
var bot = new BotInit();

bot.initReceiver(token);





var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();