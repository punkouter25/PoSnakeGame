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
using Blazored.LocalStorage; // Added for local storage
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for the app with a relative URL for when hosted in API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Azure Configuration Service (for retrieving Azure Function URL and other settings)
builder.Services.AddScoped<AzureConfigurationService>();

// Determine if we're running in production (Azure) or development
bool isProduction = builder.HostEnvironment.IsProduction();
Console.WriteLine($"Environment: {(isProduction ? "Production" : "Development")}");

// Configure Azure Table Storage from appsettings.json
var tableConfig = new TableStorageConfig();
var storageConfig = builder.Configuration.GetSection("TableStorage");
tableConfig.ConnectionString = storageConfig["ConnectionString"];
tableConfig.HighScoresTableName = storageConfig["HighScoresTableName"];
tableConfig.GameStatisticsTableName = storageConfig["GameStatisticsTableName"];

Console.WriteLine($"Using Azure Table Storage: {(tableConfig.IsUsingLocalStorage ? "Local Azurite" : "Azure Cloud")}");
builder.Services.AddSingleton(tableConfig);

// Register game services
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameEngine>();

// Configure API base URL differently when hosted in API vs standalone
// When hosted in the API project, we use a relative URL
Console.WriteLine($"Base Address: {builder.HostEnvironment.BaseAddress}");
string apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
    ? "http://localhost:5289/api/" // Use absolute URL in development
    : "/api/"; // When hosted, use relative URL

// The service endpoints will be configured using the relative API path
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<HelloWorldService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var service = new HelloWorldService(logger, httpClient);
    service.ConfigureHttpClient(apiBaseUrl);
    return service;
});

builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<GameStatisticsService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var service = new GameStatisticsService(logger, httpClient);
    service.ConfigureHttpClient(apiBaseUrl);
    return service;
});

// Register the HighScoreService
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<HighScoreService>>();
    var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var service = new HighScoreService(logger, httpClient);
    service.ConfigureHttpClient(apiBaseUrl);
    return service;
});

// Add Blazored Local Storage services
builder.Services.AddBlazoredLocalStorage(); 

// Add user preferences service (using Scoped as it interacts with LocalStorage)
builder.Services.AddScoped<IUserPreferencesService, LocalStorageUserPreferencesService>();

// Add Sound Service (changed to Singleton as GameService is Singleton and IJSRuntime is safe)
builder.Services.AddSingleton<ISoundService, SoundService>();

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Set minimum level to Debug for more verbose output
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Register DiagnosticsService
builder.Services.AddScoped<DiagnosticsService>();

// Build the app
var app = builder.Build();

await app.RunAsync();
