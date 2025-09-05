#!/usr/bin/env pwsh

# Parameters must be declared at the top of the script
param([string[]]$Service)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-ComposeV2 {
  try { $null = & docker compose version 2>$null; return $LASTEXITCODE -eq 0 } catch { return $false }
}
$UseV2 = Test-ComposeV2
function Compose { param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Args) if ($UseV2) { & docker compose @Args } else { & docker-compose @Args } }

if ($Service -and $Service.Count -gt 0) {
  Write-Host "Stopping services: $($Service -join ', ')"
  Compose stop @Service
} else {
  Write-Host "Stopping all services in compose project..."
  Compose stop
}

Write-Host
Compose ps
