# Post-provision script to configure apphost.settings.json
# This script reads environment variables from azd and populates the apphost.settings.json file

$ErrorActionPreference = "Stop"

Write-Host "Configuring apphost.settings.json..." -ForegroundColor Green

# Get the root directory (two levels up from the script)
$ROOT_DIR = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$SETTINGS_FILE = Join-Path $ROOT_DIR "apphost.settings.json"
$TEMPLATE_FILE = Join-Path $ROOT_DIR "apphost.settings.template.json"

# Check if settings file exists, if not, copy from template
if (-not (Test-Path $SETTINGS_FILE)) {
    Write-Host "apphost.settings.json not found. Copying from template..." -ForegroundColor Yellow
    if (Test-Path $TEMPLATE_FILE) {
        Copy-Item $TEMPLATE_FILE $SETTINGS_FILE
        Write-Host "Template copied successfully." -ForegroundColor Green
    } else {
        Write-Host "Error: Template file not found at $TEMPLATE_FILE" -ForegroundColor Red
        exit 1
    }
}

# Read environment variables from azd
$azdEnvOutput = azd env get-values
$envVars = @{}
foreach ($line in $azdEnvOutput) {
    if ($line -match '^([^=]+)=(.*)$') {
        $envVars[$matches[1]] = $matches[2] -replace '^"?(.*?)"?$', '$1'
    }
}

$COSMOS_NAME = if ($envVars.ContainsKey('AZURE_COSMOS_NAME')) { $envVars['AZURE_COSMOS_NAME'] } else { "" }
$COSMOS_RESOURCE_GROUP = if ($envVars.ContainsKey('AZURE_COSMOS_RESOURCE_GROUP')) { $envVars['AZURE_COSMOS_RESOURCE_GROUP'] } else { "" }

# Validate required variables

if ([string]::IsNullOrEmpty($COSMOS_NAME)) {
    Write-Host "Warning: AZURE_COSMOS_NAME environment variable is not set" -ForegroundColor Yellow
    $COSMOS_NAME = ""
}

if ([string]::IsNullOrEmpty($COSMOS_RESOURCE_GROUP)) {
    Write-Host "Warning: AZURE_COSMOS_RESOURCE_GROUP environment variable is not set" -ForegroundColor Yellow
    $COSMOS_RESOURCE_GROUP = ""
}

# Update the settings file
try {
    $settingsContent = Get-Content $SETTINGS_FILE -Raw | ConvertFrom-Json
    $settingsContent.Parameters.cosmosName = $COSMOS_NAME
    $settingsContent.Parameters.cosmosResourceGroup = $COSMOS_RESOURCE_GROUP
    $settingsContent | ConvertTo-Json -Depth 10 | Set-Content $SETTINGS_FILE
} catch {
    Write-Host "Error updating settings file: $_" -ForegroundColor Red
    exit 1
}

Write-Host "apphost.settings.json configured successfully!" -ForegroundColor Green
Write-Host "  - Cosmos Name: $COSMOS_NAME" -ForegroundColor Cyan
Write-Host "  - Cosmos Resource Group: $COSMOS_RESOURCE_GROUP" -ForegroundColor Cyan
