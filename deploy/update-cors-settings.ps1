param(
    [string]$functionAppName = "posnakegame-functions",
    [string]$resourceGroupName = "PoSnakeGame",
    [bool]$localDevelopment = $true
)

if ($localDevelopment) {
    Write-Host "Setting up for local development with Azurite..."
    
    # Check if we have access to the local.settings.json file
    $localSettingsPath = "../PoSnakeGame.Functions/local.settings.json"
    
    if (Test-Path $localSettingsPath) {
        $localSettings = Get-Content -Path $localSettingsPath -Raw | ConvertFrom-Json
        
        # Update CORS settings in local.settings.json
        $corsOrigins = @(
            "http://localhost:5000",
            "http://localhost:5001",
            "https://localhost:5000", 
            "https://localhost:5001",
            "http://localhost:5297",
            "https://localhost:7047",
            "http://127.0.0.1:5000",
            "http://127.0.0.1:5001",
            "https://127.0.0.1:5000", 
            "https://127.0.0.1:5001",
            "http://127.0.0.1:5297",
            "https://127.0.0.1:7047"
        ) -join ","
        
        # Update local settings file
        $localSettings.Host.CORS = $corsOrigins
        $localSettings.Host.CORSCredentials = $true
        
        # Write back the updated settings
        $localSettings | ConvertTo-Json -Depth 10 | Set-Content -Path $localSettingsPath
        
        Write-Host "Updated local.settings.json with CORS settings for local development"
        Write-Host "Make sure your application is configured to use http://localhost:7071 for the Function App endpoint"
    }
    else {
        Write-Host "Could not find local.settings.json file at $localSettingsPath" -ForegroundColor Red
    }
}
else {
    # Set the allowed origins for cloud deployment
    $allowedOrigins = @(
        "http://localhost:5000",
        "http://localhost:5001",
        "https://localhost:5000", 
        "https://localhost:5001",
        "http://localhost:5297",
        "https://localhost:7047",
        "https://zealous-river-059a32e0f.6.azurestaticapps.net",
        "https://posnakegame-web.azurestaticapps.net"
    )

    Write-Host "Updating CORS settings for Function App $functionAppName..."

    # Convert the origins array to a space-separated string as required by Az CLI
    $originsString = $allowedOrigins -join " "

    # Update CORS settings
    az functionapp cors add --name $functionAppName --resource-group $resourceGroupName --allowed-origins $allowedOrigins

    # Also set the CORS settings using the web config approach as a backup
    az functionapp config appsettings set --name $functionAppName --resource-group $resourceGroupName --settings "CORS_ALLOWED_ORIGINS=$originsString"

    # Enable CORS credentials
    az functionapp config set --name $functionAppName --resource-group $resourceGroupName --cors-credentials true

    Write-Host "CORS settings updated successfully!"
}

# For local development, let's also ensure that CORS is enabled via direct file modification
if ($localDevelopment) {
    # Try to find the Functions runtime process and stop it first to avoid file locking
    $funcProcess = Get-Process -Name "func" -ErrorAction SilentlyContinue
    if ($funcProcess) {
        Write-Host "Stopping Azure Functions runtime to update settings..."
        Stop-Process -Name "func" -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }
    
    # Ensure proper CORS headers are added to responses directly in host.json
    $hostJsonPath = "../PoSnakeGame.Functions/host.json"
    if (Test-Path $hostJsonPath) {
        Write-Host "Updating host.json CORS settings..."
        $hostJson = Get-Content -Path $hostJsonPath -Raw | ConvertFrom-Json
        
        # Ensure the cors section exists
        if (-not $hostJson.PSObject.Properties['cors']) {
            $hostJson | Add-Member -MemberType NoteProperty -Name "cors" -Value @{}
        }
        
        # Add allowed origins
        $hostJson.cors | Add-Member -MemberType NoteProperty -Name "allowedOrigins" -Value @(
            "http://localhost:5000",
            "http://localhost:5001",
            "https://localhost:5000", 
            "https://localhost:5001",
            "http://localhost:5297",
            "https://localhost:7047",
            "http://127.0.0.1:5000",
            "http://127.0.0.1:5001",
            "https://127.0.0.1:5000", 
            "https://127.0.0.1:5001",
            "http://127.0.0.1:5297",
            "https://127.0.0.1:7047",
            "*" # Adding wildcard as a fallback
        ) -Force
        
        # Enable CORS credentials
        $hostJson.cors | Add-Member -MemberType NoteProperty -Name "supportCredentials" -Value $true -Force
        
        # Write changes back to file
        $hostJson | ConvertTo-Json -Depth 10 | Set-Content -Path $hostJsonPath
        
        Write-Host "host.json updated with CORS settings"
    }
}

# Display configuration message
if ($localDevelopment) {
    Write-Host "Local development environment configured."
    Write-Host "Ensure your application is using: http://localhost:7071/api/ for Function App endpoints."
    Write-Host "Restart your Azure Functions host to apply the changes."
} else {
    Write-Host "Cloud Function App CORS settings configured."
    Write-Host "Ensure your application is using: https://$functionAppName.azurewebsites.net/api/ for Function App endpoints."
}