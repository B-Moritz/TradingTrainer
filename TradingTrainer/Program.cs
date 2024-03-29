using Microsoft.EntityFrameworkCore;
using TradingSchemaSp;
using TradingTrainer.BLL;
using TradingTrainer.DAL;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddDbContext<TradingContext>();
builder.Services.AddScoped<ITradingRepository, TradingRepository>();
builder.Services.AddScoped<ISearchResultRepositry, SearchResultRepositry>();
builder.Services.AddScoped<ITradingService, TradingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IInputValidationService, InputValidationService>();
//Adding sessions
builder.Services.AddSession(options => {
    options.Cookie.Name = "TradingTrainer.Session";
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddDistributedMemoryCache();
// Adding timed background service to clean the database once per day
builder.Services.AddHostedService<TradingSchemaWorker>();

var app = builder.Build();
// Adding logging capabilities
ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
loggerFactory.AddFile("Logs/TradingLog.txt");

//app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.UseSession();

app.Run();



/*app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();*/