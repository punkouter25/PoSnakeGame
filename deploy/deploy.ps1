# Deploy Azure Static Web App for PoSnakeGame

param(
    [Parameter(Mandatory=$true)]
    [string]$resourceGroupName = "PoSnakeGame",
    [string]$location = "eastus",
    [string]$storageAccountName = "posnakegamestorage",
    [string]$functionAppName = "posnakegame-functions",
    [string]$staticWebAppName = "posnakegame-web"
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

# Create Resource Group
Write-Host "Creating Resource Group..."
az group create --name $resourceGroupName --location $location

# Create Storage Account
Write-Host "Creating Storage Account..."
az storage account create `
    --name $storageAccountName `
    --resource-group $resourceGroupName `
    --location $location `
    --sku Standard_LRS `
    --kind StorageV2

# Get storage account connection string
$storageConnectionString = $(az storage account show-connection-string `
    --name $storageAccountName `
    --resource-group $resourceGroupName `
    --query connectionString `
    --output tsv)

# Create Function App with consumption plan
Write-Host "Creating Function App..."
az functionapp create `
    --name $functionAppName `
    --resource-group $resourceGroupName `
    --storage-account $storageAccountName `
    --runtime dotnet `
    --runtime-version 8 `
    --functions-version 4 `
    --os-type Windows `
    --consumption-plan-location $location

# Configure Function App settings
Write-Host "Configuring Function App settings..."
az functionapp config appsettings set `
    --name $functionAppName `
    --resource-group $resourceGroupName `
    --settings "AzureWebJobsStorage=$storageConnectionString" `
    "TableStorageConnectionString=$storageConnectionString"

# Build the Function App
Write-Host "Building the Function App..."
dotnet build "$rootDir\PoSnakeGame.Functions\PoSnakeGame.Functions.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Function App build failed"
    exit 1
}

# Deploy Function App
Write-Host "Deploying Function App..."
dotnet publish "$rootDir\PoSnakeGame.Functions\PoSnakeGame.Functions.csproj" -c Release
az functionapp deployment source config-zip `
    --name $functionAppName `
    --resource-group $resourceGroupName `
    --src "$rootDir\PoSnakeGame.Functions\bin\Release\net8.0\publish.zip"

# Create Static Web App
Write-Host "Creating and deploying Static Web App..."
# Build the Blazor WebAssembly project
Write-Host "Building the Blazor WebAssembly project..."
dotnet build "$rootDir\PoSnakeGame.Wa\PoSnakeGame.Wa.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Blazor Web App build failed"
    exit 1
}

# Deploy Static Web App
az staticwebapp create `
    --name $staticWebAppName `
    --resource-group $resourceGroupName `
    --location $location `
    --source "$rootDir" `
    --app-location "PoSnakeGame.Wa" `
    --output-location "wwwroot" `
    --api-location "PoSnakeGame.Functions"

Write-Host "`nDeployment completed successfully!"
Write-Host "Function App URL: https://$functionAppName.azurewebsites.net"
Write-Host "Static Web App URL: https://$staticWebAppName.azurestaticapps.net"
