using Microsoft.EntityFrameworkCore;
using TradingSchemaSp;
using Webapplikasjoner_oblig.DAL;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddDbContext<TradingContext>();
builder.Services.AddScoped<ITradingRepository, TradingRepository>();
builder.Services.AddScoped<ISearchResultRepositry, SearchResultRepositry>();
// Adding timed background service to clean the database once per day
builder.Services.AddHostedService<TradingSchemaWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();
*/


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

/*
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");*/
app.MapControllers();
app.MapFallbackToFile("index.html"); ;

app.Run();