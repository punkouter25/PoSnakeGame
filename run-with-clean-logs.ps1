$logsPath = Join-Path $PSScriptRoot "PoSnakeGame.Web\Logs"

# Create logs directory if it doesn't exist
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath | Out-Null
    Write-Host "Created Logs directory"
}
else {
    # Clear all log files
    Get-ChildItem -Path $logsPath -Filter "*.log" | ForEach-Object {
        Clear-Content -Path $_.FullName
        Write-Host "Cleared log file: $($_.Name)"
    }
}

# Run the application
Write-Host "Starting application..."
dotnet run --project PoSnakeGame.Web