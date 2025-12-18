#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

# Write Vite backend URL for production builds so the SPA calls the real backend.
$rootDir = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
$frontendDir = Join-Path $rootDir "src/frontend"
$envFile = Join-Path $frontendDir ".env.production"

$backendUrl = azd env get-value BACKEND_URL
if (-not $backendUrl) {
    Write-Error "BACKEND_URL is not set in the azd environment; cannot configure frontend API base"
}

"VITE_BACKEND_URL=$backendUrl" | Set-Content -Path $envFile -NoNewline
Write-Host "Wrote backend URL to $envFile"
