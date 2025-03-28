using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        
        // Register any additional services here
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddApplicationInsights();
        });
        
        // Ensure services are registered with appropriate lifetimes
        // Singleton for services that should live for the entire application lifetime
        // Scoped for services that should be created once per function execution
        // Transient for services that should be created each time they're requested
    })
    .Build();

host.Run();
