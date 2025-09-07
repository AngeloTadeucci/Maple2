#!/usr/bin/env pwsh

param(
  [int[]]$NonInstancedChannels,
  [switch]$IncludeInstanced,
  [switch]$NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Test-ComposeV2 {
  try { $null = & docker compose version 2>$null; return $LASTEXITCODE -eq 0 } catch { return $false }
}
$UseV2 = Test-ComposeV2

function Compose {
  param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Args)
  if ($UseV2) { & docker compose @Args } else { & docker-compose @Args }
}

function Wait-Healthy {
  param(
    [Parameter(Mandatory=$true)][string]$Service,
    [int]$TimeoutSec = 300,
    [switch]$Soft
  )
  Write-Host "Waiting for $Service to be healthy (timeout ${TimeoutSec}s)..."
  $start = Get-Date
  while ($true) {
    $cid = Compose ps -q $Service 2>$null | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($cid)) { Start-Sleep -Seconds 2; continue }

    $statusRaw = docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}' $cid 2>$null
    $status = ("$statusRaw" | Out-String).Trim().ToLowerInvariant()

    if ($status -match 'healthy') {
      Write-Host "OK: $Service is healthy"
      return $true
    } elseif ($status -match '^(running|starting|created)$') {
    } elseif ($status -match 'exited') {
      Write-Warning "$Service exited unexpectedly. Showing last logs:"
      try { Compose logs --no-color --tail=200 $Service } catch { }
      if ($Soft) { return $false } else { throw "$Service exited" }
    }

    if ((Get-Date) - $start -gt [TimeSpan]::FromSeconds($TimeoutSec)) {
      Write-Warning "Timeout waiting for $Service to be healthy. Logs:"
      try { Compose logs --no-color --tail=200 $Service } catch { }
      if ($Soft) { return $false } else { throw "Timeout waiting for $Service" }
    }
    Start-Sleep -Seconds 2
  }
}

function Get-DotEnv {
  param([string]$Path = ".env")
  $map = @{}
  if (Test-Path $Path) {
    foreach ($line in Get-Content $Path) {
      if ($line -match '^(\s*#|\s*$)') { continue }
      $kv = $line -split '=',2
      if ($kv.Count -eq 2) { $map[$kv[0].Trim()] = $kv[1].Trim() }
    }
  }
  return $map
}

$envMap = Get-DotEnv
$gameIp = $envMap['GAME_IP']
if (-not $gameIp) {
  Write-Warning "GAME_IP is not set in .env. Clients may receive 127.0.0.1 and fail to connect."
} elseif ($gameIp -match '^(127\.0\.0\.1|localhost)$') {
  Write-Warning "GAME_IP is set to $gameIp. External clients will fail. Set GAME_IP to your host/LAN IP in .env."
}

if (-not $PSBoundParameters.ContainsKey('NonInstancedChannels') -or -not $NonInstancedChannels) {
  $NonInstancedChannels = @(1)
}
if (-not $PSBoundParameters.ContainsKey('IncludeInstanced')) {
  $IncludeInstanced = $true
}

if (-not $NoBuild) {
  Write-Host "Building images..."
  Compose build --pull
}

Write-Host "Starting database..."
Compose @('up','--detach','mysql')
Wait-Healthy -Service mysql -TimeoutSec 300

Write-Host "Starting world, login, and web..."
Compose @('up','--detach','world','login','web')
Wait-Healthy -Service world -TimeoutSec 300
Wait-Healthy -Service login -TimeoutSec 300

Write-Host "Starting game channels..."
$started = @()
if ($IncludeInstanced) {
  Compose @('up','--detach','game-ch0')
  $null = Wait-Healthy -Service game-ch0 -TimeoutSec 300 -Soft
  $started += 'game-ch0'
}

foreach ($ch in $NonInstancedChannels) {
  $svc = "game-ch$ch"
  Compose @('up','--detach',$svc)
  $null = Wait-Healthy -Service $svc -TimeoutSec 300 -Soft
  $started += $svc
}

Write-Host
Compose ps
Write-Host
Write-Host "All services started. Tail logs with:"
$joined = ($started + @('world','login')) -join ' '
if ($UseV2) { Write-Host "  docker compose logs -f $joined" }
else { Write-Host "  docker-compose logs -f $joined" }
