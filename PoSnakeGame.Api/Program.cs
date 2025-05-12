using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure; // For AddAzureClients
using System.IO; // Add this for Path
using Serilog; // Add Serilog namespace
using Serilog.Events; // Add for LogEventLevel

// --- Serilog File Logging Setup ---
// Configure Serilog logger *before* creating the builder
// Place log.txt in the solution root for easier access and debugging
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "log.txt"); 

// Always create a new log file on startup (not append to existing)
if (File.Exists(logFilePath))
{
    try
    {
        File.Delete(logFilePath);
    }
    catch
    {
        // If file is locked or cannot be deleted, use a timestamp in the name
        logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", $"log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
    }
}

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Reduce noise specifically from ASP.NET Core
    .Enrich.FromLogContext()
    .WriteTo.Console() // Requires Serilog.Sinks.Console package
    .WriteTo.File(logFilePath, 
                  rollingInterval: RollingInterval.Infinite, // Don't roll the file by time period
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}") // Single log file
    .CreateLogger();

try
{
    Log.Information("----- PoSnakeGame.Api Starting Up -----");
    Log.Information("Log file created at: {LogPath}", logFilePath);

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging *instead* of default Microsoft logging
    builder.Host.UseSerilog(); // Requires Serilog.AspNetCore package

    // --- Get Logger Instance (using Serilog's static Log class for early logging) ---
    var logger = Log.ForContext<Program>(); // Use Serilog's static Log class
    logger.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

    // --- CORS Configuration ---
    var allowedOrigins = new[]
    {
        "http://localhost:5000",
        "http://localhost:5001",
        "https://localhost:5000",
        "https://localhost:5001",
        "http://localhost:5297", // Default Blazor WASM debug port
        "https://localhost:7047", // Default Blazor WASM debug port (HTTPS) - Check launchSettings.json
        "http://127.0.0.1:5000",
        "http://127.0.0.1:5001",
        "https://127.0.0.1:5000",
        "https://127.0.0.1:5001",
        "http://127.0.0.1:5297",
        "https://127.0.0.1:7047",
        "https://ashy-water-0fe4f090f.6.azurestaticapps.net", // Add deployed SWA URL from error
        "https://posnakegame-web.azurestaticapps.net" // Keep existing actual deployed frontend URL
    };

    logger.Information("Configuring CORS. Allowed Origins: {Origins}", string.Join(", ", allowedOrigins));

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Important for SignalR or auth cookies
                logger.Information("CORS policy 'AllowSpecificOrigins' configured.");
            });
    });

    // Add services to the container.
    // builder.Services.AddApplicationInsightsTelemetry(); // Serilog can sink to AppInsights if needed later
    builder.Services.AddControllers()
        .AddApplicationPart(typeof(Program).Assembly); // Explicitly add this assembly as application part

    // Add Razor Pages support for serving the Blazor WebAssembly app
    builder.Services.AddRazorPages();

    // --- Configure and Register TableStorageConfig ---
    logger.Information("Configuring Table Storage services...");
    var tableStorageConfig = builder.Configuration.GetSection("TableStorage").Get<TableStorageConfig>();
    if (tableStorageConfig == null)
    {
        var errorMsg = "TableStorage configuration section is missing or invalid. Defaulting to development storage.";
        logger.Error(errorMsg);
        // Create a default config if missing, relying on the default connection string within the class
        tableStorageConfig = new TableStorageConfig(); 
    }
    
    // Log whether local storage is being used based on the config instance
    logger.Information("TableStorageConfig loaded: HighScoresTable={HSTN}, GameStatsTable={GSTN}, IsUsingLocalStorage={IsLocal}", 
        tableStorageConfig.HighScoresTableName, 
        tableStorageConfig.GameStatisticsTableName,
        tableStorageConfig.IsUsingLocalStorage); // Use the correct property name

    builder.Services.AddSingleton(tableStorageConfig); 
    builder.Services.AddSingleton<ITableStorageService, TableStorageService>();

    // Register Azure Clients (including TableServiceClient)
    builder.Services.AddAzureClients(clientBuilder =>
    {
        // Use the ConnectionString property from the resolved config object
        var connectionString = tableStorageConfig.ConnectionString; 
        logger.Information("Registering TableServiceClient. IsUsingLocalStorage: {IsLocal}", tableStorageConfig.IsUsingLocalStorage);
       
        // Register TableServiceClient using connection string from configuration OR let TableStorageService handle dev storage
         clientBuilder.AddTableServiceClient(connectionString); // Provide the resolved connection string

        // TODO: Add configuration for Managed Identity if needed in Azure environment
        // clientBuilder.UseCredential(new DefaultAzureCredential()); 
    });
    logger.Information("Table Storage services configured.");


    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        logger.Information("Development environment detected. Enabling Swagger UI and Developer Exception Page.");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage(); // More detailed errors in dev
        app.UseWebAssemblyDebugging(); // Add WebAssembly debugging support
    }
    else
    {
        // Add production error handling middleware if needed
        app.UseExceptionHandler("/Error");
        app.UseHsts(); // Enforce HTTPS in production
    }

    app.UseHttpsRedirection();
    logger.Information("HTTPS redirection enabled.");

    // Serve static files and use Blazor framework files
    app.UseBlazorFrameworkFiles(); // Serve Blazor framework files
    app.UseStaticFiles(); // Serve static files from wwwroot
    logger.Information("Static files and Blazor framework files middleware configured.");

    // --- Apply CORS Policy ---
    // IMPORTANT: Place UseCors before UseAuthorization
    app.UseCors("AllowSpecificOrigins");
    logger.Information("CORS policy 'AllowSpecificOrigins' applied.");

    // Routing setup
    app.UseRouting();

    // Add authorization middleware if needed in the future
    // app.UseAuthorization();

    // --- Map Controllers ---
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers(); // Map attribute-routed API controllers
        endpoints.MapRazorPages(); // Map Razor Pages (needed for Blazor hosting)
        
        // Serve the WebAssembly app from the root URL
        endpoints.MapFallbackToFile("index.html");
    });
    
    logger.Information("Controller endpoints and fallback route mapped.");

    logger.Information("----- PoSnakeGame.Api Application Build Complete - Running... -----");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "----- PoSnakeGame.Api Host terminated unexpectedly -----");
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are written before exiting
}
