using PoSnakeGame.Infrastructure.Configuration;
using PoSnakeGame.Infrastructure.Services;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure; // For AddAzureClients

var builder = WebApplication.CreateBuilder(args);

// --- CORS Configuration ---
var allowedOrigins = new[]
{
    "http://localhost:5000",
    "http://localhost:5001",
    "https://localhost:5000",
    "https://localhost:5001",
    "http://localhost:5297", // Default Blazor WASM debug port
    "https://localhost:7047", // Default Blazor WASM debug port (HTTPS)
    "http://127.0.0.1:5000",
    "http://127.0.0.1:5001",
    "https://127.0.0.1:5000",
    "https://127.0.0.1:5001",
    "http://127.0.0.1:5297",
    "https://127.0.0.1:7047",
    "https://zealous-river-059a32e0f.6.azurestaticapps.net", // Example deployed frontend URL
    "https://posnakegame-web.azurestaticapps.net" // Actual deployed frontend URL
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Important for SignalR or auth cookies
        });
});

// Add services to the container.
builder.Services.AddControllers(); // Add support for API controllers

// --- Configure and Register TableStorageConfig ---
// Read the config section
var tableStorageConfig = builder.Configuration.GetSection("TableStorage").Get<TableStorageConfig>();
if (tableStorageConfig == null)
{
    // Handle error: configuration section missing or invalid
    throw new InvalidOperationException("TableStorage configuration section is missing or invalid.");
}
// Register the config object itself as a singleton
builder.Services.AddSingleton(tableStorageConfig); 

// Register TableStorageService (which depends on TableStorageConfig)
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();

// Register Azure Clients (including TableServiceClient)
builder.Services.AddAzureClients(clientBuilder =>
{
    // Register TableServiceClient using connection string from configuration
    clientBuilder.AddTableServiceClient(builder.Configuration.GetConnectionString("TableStorage") ?? 
                                        builder.Configuration["TableStorage:ConnectionString"]); 
                                        // Fallback to section key if GetConnectionString returns null

    // TODO: Add configuration for Managed Identity if needed in Azure environment
    // clientBuilder.UseCredential(new DefaultAzureCredential()); 
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Apply CORS Policy ---
// IMPORTANT: Place UseCors before UseAuthorization
app.UseCors("AllowSpecificOrigins");

// Add authorization middleware if needed in the future
// app.UseAuthorization();

// --- Map Controllers ---
app.MapControllers(); // Map attribute-routed API controllers

app.Run();
