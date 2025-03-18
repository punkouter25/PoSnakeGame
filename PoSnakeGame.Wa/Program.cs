using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using PoSnakeGame.Core.Interfaces;
using PoSnakeGame.Core.Services;
using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;
using PoSnakeGame.Wa;
using PoSnakeGame.Wa.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

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

// Use the mock table storage service for WebAssembly
builder.Services.AddSingleton<ITableStorageService, MockTableStorageService>();

// Add user preferences service
builder.Services.AddSingleton<IUserPreferencesService, LocalStorageUserPreferencesService>();

// Configure logging
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

await builder.Build().RunAsync();
