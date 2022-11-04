// Webapplikasjoner oblig 1     OsloMet     31.10.2022

// This file contains code defining the worker object that should clean the database once a day.
// It removes old resources that are not used by any user.

// Resources used: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=netcore-cli

using TradingTrainer.DAL;
using System.Diagnostics;

namespace TradingSchemaSp;


public class TradingSchemaWorker : IHostedService, IDisposable
{
    private IServiceProvider _service;
    // The timer used to scedule a task once a day
    private Timer? _timer = null;

    public TradingSchemaWorker(IServiceProvider service)
    {
        _service = service;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        // calculate the amount of seconds untill midnight (local time)
        int startTime = 0;
        DateTime curTime = DateTime.Now;
        startTime += (60 - curTime.Second);
        startTime += (60 - curTime.Minute - 1) * 60;
        startTime += (24 - curTime.Hour - 1) * 60 * 60;

        // Initial cleanup:
        CleanSearchResults(null);

        Debug.WriteLine($"TradingSchemaWorker: Timed hosted service starting in {startTime} seconds");
        // Start the timer with a time intervall of 24 hours. The first trigger should be at 00:00:00 local time.
        _timer = new Timer(CleanSearchResults, null, TimeSpan.FromSeconds(startTime), TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    public async void CleanSearchResults(Object? state)
    {
        Debug.WriteLine($"TradingSchemaWorker: Executing Cleanup {DateTime.Now}");
        // Start a timer
        int start = DateTime.Now.Millisecond;
        using (var scope = _service.CreateScope()) {
            ITradingRepository curTradingRepo = scope.ServiceProvider.GetRequiredService<ITradingRepository>();
            try
            {
                // Try to clean the database
                await curTradingRepo.CleanTradingSchemaAsync();
            }
            catch (Exception ex)
            {
                // Log that the Clean operation failed.
                Debug.WriteLine(ex.Message);
            }
        }
        // Take the end time
        int stop = DateTime.Now.Millisecond;
        Debug.WriteLine($"TradingSchemaWorker: Executed Cleanup in {(stop - start):N} milliseconds");
    }

    public Task StopAsync(CancellationToken stoppingToken) 
    {
        Debug.WriteLine("TradingSchemaWorker: Stopping hosted service");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
