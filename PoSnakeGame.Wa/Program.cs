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

// Add Azure Configuration Service (for retrieving Azure Function URL and other settings)
builder.Services.AddScoped<AzureConfigurationService>();

// Determine if we're running in production (Azure) or development
bool isProduction = builder.HostEnvironment.IsProduction();
Console.WriteLine($"Environment: {(isProduction ? "Production" : "Development")}");

// Configure Azure Table Storage
var tableConfig = new TableStorageConfig();
if (isProduction)
{
    // In production, the actual connection string is managed by the Azure Functions
    // We only store a placeholder here for type initialization
    tableConfig.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=posnakegamestorage;AccountKey=****;EndpointSuffix=core.windows.net";
    tableConfig.HighScoresTableName = "HighScores";
    tableConfig.GameStatisticsTableName = "GameStatistics";
    Console.WriteLine("Using production Azure Storage via Azure Functions");
}
else
{
    // In development, use the local storage emulator
    tableConfig.ConnectionString = "UseDevelopmentStorage=true";
    tableConfig.HighScoresTableName = "HighScores";
    tableConfig.GameStatisticsTableName = "GameStatistics";
    Console.WriteLine("Using local Azurite storage emulator");
}
builder.Services.AddSingleton(tableConfig);

// Register game services
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameEngine>();

// The service endpoints will be dynamically configured at runtime based on environment
// This registration approach uses the Factory pattern to create services with the right configuration
builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<HelloWorldService>>();
    // Create a base HttpClient that will get its URL configured at runtime
    var httpClient = new HttpClient();
    // Return the service with the dynamically configured HttpClient
    return new HelloWorldService(logger, httpClient);
});

builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<GameStatisticsService>>();
    var httpClient = new HttpClient();
    return new GameStatisticsService(logger, httpClient);
});

builder.Services.AddScoped(sp => 
{
    var logger = sp.GetRequiredService<ILogger<PoSnakeGame.Wa.Services.TableStorageService>>();
    var httpClient = new HttpClient();
    return new PoSnakeGame.Wa.Services.TableStorageService(logger, httpClient);
});

// Use the mock table storage service for WebAssembly
builder.Services.AddScoped<ITableStorageService>(sp => sp.GetRequiredService<PoSnakeGame.Wa.Services.TableStorageService>());

// Add user preferences service
builder.Services.AddSingleton<IUserPreferencesService, LocalStorageUserPreferencesService>();

// Configure logging
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Build the app
var app = builder.Build();

// Configure service URLs based on the environment
// This must be done after the app is built to resolve the AzureConfigurationService
var serviceProvider = app.Services;
var azureConfigService = serviceProvider.GetRequiredService<AzureConfigurationService>();
var functionBaseUrl = await azureConfigService.GetFunctionAppBaseUrlAsync();

// Update the URLs for all HTTP clients in services
UpdateServiceHttpClients(serviceProvider, functionBaseUrl);

await app.RunAsync();

// Helper method to update all HTTP clients with the correct base URL
// This demonstrates the Strategy pattern by dynamically configuring behavior at runtime
void UpdateServiceHttpClients(IServiceProvider services, string baseUrl)
{
    // Get all the services that need their HTTP clients configured
    var helloWorldService = services.GetRequiredService<HelloWorldService>();
    var gameStatisticsService = services.GetRequiredService<GameStatisticsService>();
    var mockTableStorageService = services.GetRequiredService<PoSnakeGame.Wa.Services.TableStorageService>();
    
    // Ensure baseUrl ends with a slash for proper URL combination
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    
    // All services should use the api/ base path
    var apiBaseUrl = baseUrl + "api/";
    
    // Log for debugging
    Console.WriteLine($"API Base URL for all services: {apiBaseUrl}");
    
    // Configure all services with the API base URL
    helloWorldService.ConfigureHttpClient(apiBaseUrl);
    gameStatisticsService.ConfigureHttpClient(apiBaseUrl);
    mockTableStorageService.ConfigureHttpClient(apiBaseUrl);
    
    Console.WriteLine($"Configured all service HTTP clients with API base URL: {apiBaseUrl}");
}



