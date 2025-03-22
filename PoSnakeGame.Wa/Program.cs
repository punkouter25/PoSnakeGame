using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Interfaces;
using PoSnakeGame.Core.Services;
using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;
using PoSnakeGame.Wa;
using PoSnakeGame.Wa.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register default HttpClient for the app
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure Azure Table Storage
var tableConfig = new TableStorageConfig
{
    ConnectionString = "UseDevelopmentStorage=true", // Use local emulator by default
    HighScoresTableName = "HighScores",
    GameStatisticsTableName = "GameStatistics"
};
builder.Services.AddSingleton(tableConfig);

// Register game services
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameEngine>();

// Register services with dedicated HttpClient instances
builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<HelloWorldService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:7071/") };
    return new HelloWorldService(logger, httpClient);
});

builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<GameStatisticsService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:7071/api/") };
    return new GameStatisticsService(logger, httpClient);
});

builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<MockTableStorageService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:7071/api/") };
    return new MockTableStorageService(logger, httpClient);
});

// Use the mock table storage service for WebAssembly
builder.Services.AddScoped<ITableStorageService>(sp => sp.GetRequiredService<MockTableStorageService>());

// Add user preferences service
builder.Services.AddSingleton<IUserPreferencesService, LocalStorageUserPreferencesService>();

// Configure logging
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

await builder.Build().RunAsync();
