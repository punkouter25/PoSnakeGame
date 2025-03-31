using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration; // Required for IConfiguration

namespace PoSnakeGame.Wa.Services
{
    public class DiagnosticCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Checking, Connected, Disconnected, Error
        public string Message { get; set; } = string.Empty;
    }

    public class DiagnosticsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; // Inject IConfiguration
        private readonly string? _apiBaseUrl;

        // Inject HttpClient and IConfiguration
        public DiagnosticsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            // Read ApiBaseUrl from configuration
            _apiBaseUrl = _configuration["ApiBaseUrl"]; 
             if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                Console.Error.WriteLine("ApiBaseUrl is not configured in appsettings.json or environment settings.");
                // Handle missing configuration appropriately, maybe throw or default
            }
        }

        public async Task<List<DiagnosticCheck>> RunChecksAsync()
        {
            var checks = new List<DiagnosticCheck>
            {
                new DiagnosticCheck { Name = "API Connectivity" },
                new DiagnosticCheck { Name = "Table Storage Connection (via API)" }
                // Add more checks as needed
            };

            if (string.IsNullOrEmpty(_apiBaseUrl))
            {
                 checks.ForEach(c => {
                     c.Status = "Error";
                     c.Message = "API Base URL not configured.";
                 });
                 return checks;
            }


            // --- Check 1: API Connectivity (using existing HelloWorld) ---
            var apiCheck = checks.First(c => c.Name == "API Connectivity");
            apiCheck.Status = "Checking";
            try
            {
                // Assuming HelloWorld endpoint exists at the root of the API base URL + /HelloWorld
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}HelloWorld"); // Ensure endpoint matches API
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<HelloWorldResponse>(); // Define HelloWorldResponse if needed
                    apiCheck.Status = "Connected";
                    apiCheck.Message = result?.Message ?? "API reached successfully.";
                }
                else
                {
                    apiCheck.Status = "Disconnected";
                    apiCheck.Message = $"API returned status: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                apiCheck.Status = "Error";
                apiCheck.Message = $"Failed to reach API: {ex.Message}";
            }

            // --- Check 2: Table Storage (via new API endpoint) ---
             var tableCheck = checks.First(c => c.Name == "Table Storage Connection (via API)");
            // Only run this check if the API itself is reachable
            if (apiCheck.Status == "Connected") 
            {
                tableCheck.Status = "Checking";
                try
                {
                    // Call the new health check endpoint
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}Health/CheckTableStorage");
                    var result = await response.Content.ReadFromJsonAsync<HealthCheckResponse>(); // Define HealthCheckResponse

                    if (response.IsSuccessStatusCode && result != null)
                    {
                        tableCheck.Status = result.Status ?? "Connected"; // Use status from API response
                        tableCheck.Message = result.Message ?? "Table Storage check successful.";
                    }
                    else if (result != null) // Handle cases where API returns failure status code but provides details
                    {
                         tableCheck.Status = result.Status ?? "Disconnected";
                         tableCheck.Message = result.Message ?? $"API returned status: {response.StatusCode}";
                    }
                     else
                    {
                        tableCheck.Status = "Error";
                        tableCheck.Message = $"API returned status: {response.StatusCode} with no details.";
                    }
                }
                catch (Exception ex)
                {
                    tableCheck.Status = "Error";
                    tableCheck.Message = $"Failed to check Table Storage via API: {ex.Message}";
                }
            }
             else
            {
                 tableCheck.Status = "Skipped";
                 tableCheck.Message = "Skipped because API is not connected.";
            }


            return checks;
        }

        // Helper classes for deserialization (adjust properties based on actual API response)
        private class HelloWorldResponse
        {
            public string? Message { get; set; }
        }
         private class HealthCheckResponse
        {
            public string? Status { get; set; }
            public string? Message { get; set; }
        }
    }
}
