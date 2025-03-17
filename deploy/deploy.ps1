# Deploy Azure Resources for PoSnakeGame

param(
    [Parameter(Mandatory=$false)]
    [string]$environment = "prod",
    
    [Parameter(Mandatory=$false)]
    [string]$location = "canadacentral",

    [Parameter(Mandatory=$true)]
    [string]$resourceGroupName = "PoShared"
)

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

# Check if resource group exists, create if it doesn't
$rgExists = az group exists --name $resourceGroupName
if ($rgExists -eq "false") {
    Write-Host "Creating resource group $resourceGroupName..."
    az group create --name $resourceGroupName --location $location
}

# Deploy Bicep template
Write-Host "Deploying Azure resources..."
$deployment = az deployment group create `
    --resource-group $resourceGroupName `
    --template-file "$(Split-Path $MyInvocation.MyCommand.Path)\main.bicep" `
    --parameters environment=$environment location=$location `
    --output json | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed"
    exit 1
}

# Get the outputs
$webAppName = $deployment.properties.outputs.webAppName.value
$storageAccountName = $deployment.properties.outputs.storageAccountName.value
$storageConnectionString = $deployment.properties.outputs.storageConnectionString.value

Write-Host "`nDeployment completed successfully!"
Write-Host "Web App Name: $webAppName"
Write-Host "Storage Account Name: $storageAccountName"
Write-Host "`nConnection string has been set in the Web App configuration."

# Optional: Build and deploy the application
$buildOption = Read-Host "`nWould you like to build and deploy the application now? (y/n)"
if ($buildOption -eq "y") {
    Write-Host "Building and publishing the application..."
    
    # Build and publish
    dotnet publish ..\PoSnakeGame.Web\PoSnakeGame.Web.csproj -c Release
    
    # Deploy to Azure Web App
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Deploying to Azure Web App..."
        az webapp deploy --resource-group $resourceGroupName --name $webAppName --src-path ..\PoSnakeGame.Web\bin\Release\net9.0\publish
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`nApplication deployed successfully!"
            Write-Host "You can access your application at: https://$webAppName.azurewebsites.net"
        } else {
            Write-Error "Deployment failed"
        }
    } else {
        Write-Error "Build failed"
    }
}