using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions; // For OpenAPI/Swagger if needed later
using Microsoft.Extensions.DependencyInjection; // Added for AddSingleton, AddScoped, etc.
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration; // Added for ConfigurationBinder
using PoSnakeGame.Infrastructure.Configuration; // Added for TableStorageConfig
using PoSnakeGame.Infrastructure.Services; // Added for ITableStorageService and TableStorageService

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureOpenApi() // Configure OpenAPI/Swagger if needed later
    .ConfigureServices((context, services) => {
        // Retrieve configuration
        var configuration = context.Configuration;

        // Configure TableStorageConfig from application settings
        // Assumes settings like "TableStorageConnectionString" and "HighScoresTableName" exist
        services.Configure<TableStorageConfig>(options =>
        {
            // Use "AzureWebJobsStorage" if a specific one isn't set, as it's common for Functions
            options.ConnectionString = configuration["TableStorageConnectionString"] ?? configuration["AzureWebJobsStorage"];
            options.HighScoresTableName = configuration["HighScoresTableName"] ?? "HighScores"; // Default to "HighScores"
        });

        // Register TableStorageConfig as a singleton based on the configured options
        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TableStorageConfig>>().Value);

        // Register TableStorageService
        services.AddScoped<ITableStorageService, TableStorageService>();

        // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
        // services
        //     .AddApplicationInsightsTelemetryWorkerService()
        //     .ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
