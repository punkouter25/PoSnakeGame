# Sets up all Azure configuration settings for the PoSnakeGame
# This script configures both the Function App and Static Web App with proper settings

param(
    [string]$resourceGroupName = "PoSnakeGame",
    [string]$storageAccountName = "posnakegamestorage",
    [string]$functionAppName = "posnakegame-functions",
    [string]$staticWebAppName = "posnakegame-web"
)

# Setup error handling and logging
$ErrorActionPreference = "Stop"
$logFile = "$PSScriptRoot\..\logs\configure-settings-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
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

# Build the connection string
$storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey;EndpointSuffix=core.windows.net"

# Check if Function App exists
Write-Log "Checking function app $functionAppName..." "INFO"
$functionApp = az functionapp show --name $functionAppName --resource-group $resourceGroupName --query "name" -o tsv 2>$null
if (!$functionApp) {
    Write-Log "Function App $functionAppName not found in resource group $resourceGroupName." "ERROR"
    exit 1
}

# Configure the Function App settings
Write-Log "Configuring Function App settings..." "INFO"
az functionapp config appsettings set --name $functionAppName --resource-group $resourceGroupName --settings "AzureWebJobsStorage=$storageConnectionString" "TableStorageConnectionString=$storageConnectionString" | Out-Null
Write-Log "Function App settings updated successfully." "INFO"

# Check if Static Web App exists
Write-Log "Checking static web app $staticWebAppName..." "INFO"
$staticWebApp = az staticwebapp list --resource-group $resourceGroupName --query "[?name=='$staticWebAppName']" -o tsv
if (!$staticWebApp) {
    Write-Log "Static Web App $staticWebAppName does not exist in resource group $resourceGroupName." "ERROR"
    exit 1
}

# Configure the Static Web App settings
# Note: We don't store sensitive connection strings in Static Web App settings
# Instead, we store configuration about the Function App endpoint
Write-Log "Configuring Static Web App settings..." "INFO"

# Set the function app URL
try {
    $functionAppUrl = "https://$functionAppName.azurewebsites.net"
    
    # For Static Web Apps, we use the SWA CLI which can set app settings
    # These settings will be available as environment variables to the client app
    az staticwebapp appsettings set --name $staticWebAppName --resource-group $resourceGroupName --setting-names "FUNCTIONS_BASE_URL=$functionAppUrl" --verbose
    
    Write-Log "Static Web App settings updated successfully." "INFO"
}
catch {
    Write-Log "Error configuring Static Web App settings: $_" "ERROR"
    exit 1
}

Write-Log "All Azure settings have been configured successfully!" "INFO"
Write-Log "Function App ($functionAppName) has the storage connection string." "INFO"
Write-Log "Static Web App ($staticWebAppName) has the Function App URL configuration." "INFO"
Write-Log "NO sensitive connection strings are stored in the Static Web App settings." "INFO"