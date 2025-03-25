# Deploy Blazor WebAssembly app to existing Azure Static Web App
# This script focuses on deploying the content to an already created Static Web App
# It also properly configures the Azure Storage connection for production

param(
    [string]$resourceGroupName = "PoSnakeGame",
    [string]$storageAccountName = "posnakegamestorage",
    [string]$functionAppName = "posnakegame-functions",
    [string]$staticWebAppName = "posnakegame-web",
    [string]$deploymentEnvironment = "production" # 'production' or a custom environment name
)

# Setup error handling and logging
$ErrorActionPreference = "Stop"
$logFile = "$PSScriptRoot\..\logs\deploy-static-app-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$logDir = Split-Path -Parent $logFile
if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

function Write-Log {
    param(
        [string]$message,
        [string]$level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$level] $message"
    
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage
}

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

# Check if Azure CLI is installed
if (!(Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Log "Azure CLI is not installed. Please install it first." "ERROR"
    exit 1
}

# Check if logged in to Azure
$account = az account show --query name -o tsv 2>$null
if (!$account) {
    Write-Log "Not logged in to Azure. Please log in..." "WARN"
    az login
}

# Check if the resource group exists
$resourceGroupExists = az group exists --name $resourceGroupName
if ($resourceGroupExists -ne "true") {
    Write-Log "Resource group $resourceGroupName does not exist." "ERROR"
    exit 1
}

# Check if the static web app exists
$staticWebApp = az staticwebapp list --resource-group $resourceGroupName --query "[?name=='$staticWebAppName']" -o tsv
if (!$staticWebApp) {
    Write-Log "Static Web App $staticWebAppName does not exist in resource group $resourceGroupName." "ERROR"
    exit 1
}

# Check if storage account exists and get the key
Write-Log "Checking storage account $storageAccountName..." "INFO"
$storageAccount = az storage account show --name $storageAccountName --resource-group $resourceGroupName --query "name" -o tsv 2>$null
if (!$storageAccount) {
    Write-Log "Storage account $storageAccountName not found in resource group $resourceGroupName." "ERROR"
    exit 1
}

# Get the storage account key
Write-Log "Getting storage account key..." "INFO"
$storageKey = az storage account keys list --account-name $storageAccountName --resource-group $resourceGroupName --query "[0].value" -o tsv
if (!$storageKey) {
    Write-Log "Failed to get storage account key." "ERROR"
    exit 1
}

# Check if Function App exists
Write-Log "Checking function app $functionAppName..." "INFO"
$functionApp = az functionapp show --name $functionAppName --resource-group $resourceGroupName --query "name" -o tsv 2>$null
if (!$functionApp) {
    Write-Log "Function App $functionAppName not found in resource group $resourceGroupName." "ERROR"
    exit 1
}

# Update the Azure Function App settings with the storage account connection string
Write-Log "Updating Function App settings with the storage account connection string..." "INFO"
$storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"
az functionapp config appsettings set --name $functionAppName --resource-group $resourceGroupName --settings "AzureWebJobsStorage=$storageConnectionString" "TableStorageConnectionString=$storageConnectionString" | Out-Null

# Update the Program.cs file with the correct storage account connection string
Write-Log "Updating Program.cs with the correct storage account configuration..." "INFO"
$programCsPath = "$rootDir\PoSnakeGame.Wa\Program.cs"
$programCsContent = Get-Content $programCsPath -Raw

# Replace the placeholder connection string with the actual one
# We only use placeholder values for security - the actual storage account will use the connection string managed by Azure Functions
$programCsContent = $programCsContent -replace "tableConfig\.ConnectionString = ""DefaultEndpointsProtocol=https;AccountName=posnakegamestorage;AccountKey=YOUR_ACCOUNT_KEY;EndpointSuffix=core\.windows\.net""", "tableConfig.ConnectionString = ""DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=****;EndpointSuffix=core.windows.net"""

# Save the updated Program.cs file
Set-Content -Path $programCsPath -Value $programCsContent

# Build the Blazor WebAssembly project
Write-Log "Building the Blazor WebAssembly project..." "INFO"
$buildResult = dotnet publish "$rootDir\PoSnakeGame.Wa\PoSnakeGame.Wa.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Log "Blazor Web App build failed with exit code $LASTEXITCODE" "ERROR"
    Write-Log $buildResult "ERROR"
    exit 1
}

# Get the built app's output directory
$publishOutputDir = "$rootDir\PoSnakeGame.Wa\bin\Release\net8.0\publish\wwwroot"
if (!(Test-Path $publishOutputDir)) {
    Write-Log "Published output directory not found at: $publishOutputDir" "ERROR"
    exit 1
}

Write-Log "Successfully built the Blazor WebAssembly app" "INFO"

# Get deployment token for the Static Web App
Write-Log "Getting deployment token for the static web app..." "INFO"
$deploymentToken = az staticwebapp secrets list --name $staticWebAppName --resource-group $resourceGroupName --query "properties.apiKey" -o tsv

if (!$deploymentToken) {
    Write-Log "Failed to get deployment token for $staticWebAppName" "ERROR"
    exit 1
}

# Deploy using Azure Static Web Apps CLI
Write-Log "Deploying to Azure Static Web App using Azure Static Web Apps CLI..." "INFO"
Write-Log "Deployment environment: $deploymentEnvironment" "INFO"

# Create a temporary deployment folder to avoid the need for source control
$tempDeploymentDir = "$env:TEMP\posnakegame-deployment-$(Get-Date -Format 'yyyyMMddHHmmss')"
New-Item -ItemType Directory -Path $tempDeploymentDir -Force | Out-Null

try {
    # Copy the published content to the temp directory
    Copy-Item -Path "$publishOutputDir\*" -Destination $tempDeploymentDir -Recurse -Force

    # Deploy using SWA CLI
    $deploymentUrl = "https://$staticWebAppName.azurestaticapps.net"
    
    # Use Azure Static Web Apps CLI to deploy
    $deployCommand = "swa deploy `"$tempDeploymentDir`" --env `"$deploymentEnvironment`" --deployment-token `"$deploymentToken`""
    
    Write-Log "Running deployment command (token hidden for security)..." "INFO"
    Invoke-Expression $deployCommand
    
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Deployment failed with exit code $LASTEXITCODE" "ERROR"
        exit 1
    }
    
    Write-Log "Deployment completed successfully!" "INFO"
    Write-Log "Static Web App URL: $deploymentUrl" "INFO"
    Write-Log "Function App URL: https://$functionAppName.azurewebsites.net" "INFO"
}
catch {
    Write-Log "Error during deployment: $_" "ERROR"
    exit 1
}
finally {
    # Clean up temporary deployment directory
    if (Test-Path $tempDeploymentDir) {
        Remove-Item -Path $tempDeploymentDir -Recurse -Force
    }
}