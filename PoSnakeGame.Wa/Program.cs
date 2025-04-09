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

// Register default HttpClient for the app
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

// Get the function base URL before building the app to avoid accessing disposed services
// This avoids the "Cannot access disposed object" error with IServiceProvider
string functionBaseUrl = null;
builder.Services.AddScoped<Func<Task<string>>>(sp => async () =>
{
    if (functionBaseUrl != null)
        return functionBaseUrl;

    var configService = sp.GetRequiredService<AzureConfigurationService>();
    functionBaseUrl = await configService.GetFunctionAppBaseUrlAsync();

    // Ensure baseUrl ends with a slash for proper URL combination
    if (!string.IsNullOrEmpty(functionBaseUrl) && !functionBaseUrl.EndsWith("/"))
    {
        functionBaseUrl += "/";
    }

    return functionBaseUrl;
});

// The service endpoints will be dynamically configured at runtime based on environment
// This registration approach uses the Factory pattern to create services with the right configuration
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<HelloWorldService>>();
    // Create a base HttpClient 
    var httpClient = new HttpClient();
    return new HelloWorldService(logger, httpClient);
});

builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<GameStatisticsService>>();
    var httpClient = new HttpClient();
    return new GameStatisticsService(logger, httpClient);
});

// Register the new HighScoreService
builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<HighScoreService>>();
    var httpClient = new HttpClient(); // BaseAddress will be set by ServiceInitializer
    return new HighScoreService(logger, httpClient);
});

// Remove the mock TableStorageService registration and the interface mapping
// builder.Services.AddScoped(sp =>
// {
//     var logger = sp.GetRequiredService<ILogger<PoSnakeGame.Wa.Services.TableStorageService>>();
//     var httpClient = new HttpClient();
//     return new PoSnakeGame.Wa.Services.TableStorageService(logger, httpClient);
// });
// builder.Services.AddScoped<ITableStorageService>(sp => sp.GetRequiredService<PoSnakeGame.Wa.Services.TableStorageService>());

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

// Register a service to initialize URL configurations after the app has started
// This uses the Decorator pattern to wrap the initialization of services
builder.Services.AddScoped<ServiceInitializer>();

// Build the app
var app = builder.Build();

// Trigger URL configuration initialization but don't await it directly here
// This prevents accessing disposed services after the app has shut down
var serviceInitializer = app.Services.GetRequiredService<ServiceInitializer>();
_ = serviceInitializer.InitializeServicesAsync();

await app.RunAsync();

// Service initializer class to configure service URLs after app startup
// This implements the Decorator pattern to add functionality to existing services
public class ServiceInitializer
{
    private readonly IServiceProvider _services;
    private readonly Func<Task<string>> _getFunctionBaseUrlAsync;
    private readonly ILogger<ServiceInitializer> _logger;
    private bool _initialized = false;

    public ServiceInitializer(
        IServiceProvider services,
        Func<Task<string>> getFunctionBaseUrlAsync,
        ILogger<ServiceInitializer> logger)
    {
        _services = services;
        _getFunctionBaseUrlAsync = getFunctionBaseUrlAsync;
        _logger = logger;
    }

    public async Task InitializeServicesAsync()
    {
        if (_initialized)
            return;

        try
        {
            // Get the Function App base URL
            string baseUrl = await _getFunctionBaseUrlAsync();

            if (string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogWarning("Function App base URL is empty. Services may not work correctly.");
                return;
            }

            // All services should use the api/ base path
            var apiBaseUrl = baseUrl + "api/";

            // Log for debugging
            _logger.LogInformation($"API Base URL for all services: {apiBaseUrl}");

            // Configure all services with the API base URL
            if (_services.GetService<HelloWorldService>() is HelloWorldService helloWorldService)
            {
                helloWorldService.ConfigureHttpClient(apiBaseUrl);
            }

            if (_services.GetService<GameStatisticsService>() is GameStatisticsService gameStatisticsService)
            {
                gameStatisticsService.ConfigureHttpClient(apiBaseUrl);
            }

            // Configure the new HighScoreService
            if (_services.GetService<HighScoreService>() is HighScoreService highScoreService)
            {
                highScoreService.ConfigureHttpClient(apiBaseUrl);
            }

            // Remove configuration for the mock TableStorageService
            // if (_services.GetService<PoSnakeGame.Wa.Services.TableStorageService>() is PoSnakeGame.Wa.Services.TableStorageService tableStorageService)
            // {
            //     tableStorageService.ConfigureHttpClient(apiBaseUrl);
            // }

            _logger.LogInformation("Configured all service HTTP clients with API base URL successfully");
            _initialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring service HTTP clients");
        }
    }
}
