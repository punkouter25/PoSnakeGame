# Deploy Azure Static Web App for PoSnakeGame

param(
    [Parameter(Mandatory=$true)]
    [string]$resourceGroupName = "posnakegamer"
)

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

# Check if Azure CLI is installed
if (!(Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install it first."
    exit 1
}

# Check if logged in to Azure
$account = az account show --query name -o tsv 2>$null
if (!$account) {
    Write-Host "Not logged in to Azure. Please log in..."
    az login
}

# Build the Blazor WebAssembly project
Write-Host "Building the Blazor WebAssembly project..."
dotnet build "$rootDir\PoSnakeGame.Wa\PoSnakeGame.Wa.csproj" -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

# Deploy to Azure Static Web App
Write-Host "Deploying to Azure Static Web App..."
az staticwebapp deploy `
    --resource-group $resourceGroupName `
    --name posnakegame-staticwebapp `
    --artifact-location "$rootDir\PoSnakeGame.Wa\bin\Release\net9.0\wwwroot"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed"
    exit 1
}

Write-Host "`nApplication deployed successfully!"
Write-Host "You can access your application at: https://posnakegame-staticwebapp.azurewebsites.net"
