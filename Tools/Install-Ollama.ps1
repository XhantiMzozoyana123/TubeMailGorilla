#Requires -Version 5.1
<#
.SYNOPSIS
  One-click installer + runner for Ollama (Windows) - TubeMailGorilla edition.
.DESCRIPTION
  1. Installs Ollama via winget (Ollama.Ollama) if missing, fallback to direct download.
  2. Starts `ollama serve` if not running.
  3. Pulls llama3:latest (your app default) if missing.
  4. Smoke-tests http://localhost:11434/api/tags + /api/generate.
  Double-click (Run with PowerShell) or run from terminal.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Install-Ollama.ps1 -Model "llama3:latest" -NoPull -Yes
#>
param(
  [string]$Model = "llama3:latest",
  [switch]$NoPull,
  [switch]$NoServe,
  [switch]$Yes
)

$ErrorActionPreference = "Continue"
$PackageId = "Ollama.Ollama"
$OllamaUrl = "http://localhost:11434"

function Step($m){ Write-Host "`n==> $m" -ForegroundColor Cyan }
function Ok($m){ Write-Host "  [OK] $m" -ForegroundColor Green }
function Warn($m){ Write-Host "  [!!] $m" -ForegroundColor Yellow }

function Find-Ollama {
  $c = Get-Command ollama -ErrorAction SilentlyContinue
  if ($c) { return $c.Source }
  $paths = @(
    "$env:LOCALAPPDATA\Programs\Ollama\ollama.exe",
    "$env:ProgramFiles\Ollama\ollama.exe",
    "${env:ProgramFiles(x86)}\Ollama\ollama.exe"
  )
  foreach ($p in $paths) { if (Test-Path $p) { return $p } }
  return $null
}

function Test-Server {
  try {
    Invoke-RestMethod -Uri "$OllamaUrl/api/tags" -TimeoutSec 5 | Out-Null
    return $true
  } catch { return $false }
}

Write-Host "==============================================" -ForegroundColor Magenta
Write-Host " Ollama One-Click Installer (TubeMailGorilla)" -ForegroundColor Magenta
Write-Host "==============================================" -ForegroundColor Magenta
Write-Host " Target model: $Model  |  Server: $OllamaUrl" -ForegroundColor Gray

$ollama = Find-Ollama
if ($ollama) { Ok "Ollama found: $ollama" }
else {
  Step "Installing Ollama ($PackageId)..."
  $hasWinget = $false
  try { winget --version | Out-Null; $hasWinget = $true } catch {}
  if ($hasWinget) {
    Write-Host "Running: winget install --id=$PackageId -e --accept-source-agreements --accept-package-agreements" -ForegroundColor Gray
    try { winget install --id=$PackageId -e --accept-source-agreements --accept-package-agreements } catch {}
    Start-Sleep -Seconds 5
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    $ollama = Find-Ollama
  }
  if (-not $ollama) {
    Step "winget did not yield ollama, falling back to direct download..."
    $dl = "https://ollama.com/download/OllamaSetup.exe"
    $tmp = Join-Path $env:TEMP "OllamaSetup.exe"
    Write-Host "Downloading $dl -> $tmp" -ForegroundColor Gray
    try {
      [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
      Invoke-WebRequest -Uri $dl -OutFile $tmp -UseBasicParsing
      Ok "Downloaded. Launching installer..."
      Start-Process $tmp -Wait
      $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
      $ollama = Find-Ollama
    } catch {
      Warn "Direct download/install failed: $($_.Exception.Message)"
      Write-Host "Manual fallback: open https://ollama.com/download and run OllamaSetup.exe, then re-run this script." -ForegroundColor Yellow
      Start-Process "https://ollama.com/download"
      if (-not $Yes) { Read-Host "Press Enter to exit" | Out-Null }
      exit 1
    }
  }
  if ($ollama) { Ok "Installed: $ollama" }
  else {
    Warn "Install not detected. Open https://ollama.com/download manually."
    Start-Process "https://ollama.com/download"
    if (-not $Yes) { Read-Host "Press Enter to exit" | Out-Null }
    exit 1
  }
}

try { & ollama --version } catch { Warn "ollama --version failed: $($_.Exception.Message)" }

if (-not $NoServe) {
  Step "Ensuring Ollama server is running..."
  if (Test-Server) { Ok "Server already up at $OllamaUrl/api/tags" }
  else {
    Write-Host "Starting 'ollama serve' in background..." -ForegroundColor Gray
    try {
      Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
    } catch {
      Start-Process powershell -ArgumentList '-NoProfile -WindowStyle Hidden -Command "ollama serve"'
    }
    $up = $false
    for ($i=1; $i -le 15; $i++) {
      Start-Sleep -Seconds 2
      if (Test-Server) { $up = $true; break }
      Write-Host "  waiting for server... ($i/15)" -ForegroundColor DarkGray
    }
    if ($up) { Ok "Server is up!" } else { Warn "Server did not respond after ~30s. Check 'ollama serve' manually." }
  }
}

if (-not $NoPull) {
  Step "Ensuring model '$Model' is pulled (this is ~4-5 GB first time)..."
  try {
    & ollama pull $Model
    Ok "Model pull complete."
  } catch { Warn "ollama pull failed: $($_.Exception.Message)" }
} else {
  Write-Host "Skipping model pull (-NoPull)." -ForegroundColor Gray
}

Step "Verifying..."
try {
  $tags = Invoke-RestMethod -Uri "$OllamaUrl/api/tags" -TimeoutSec 10
  $names = @($tags.models | ForEach-Object { $_.name })
  Write-Host ("  Models: " + ($names -join ", ")) -ForegroundColor Gray
  if ($names -contains $Model) { Ok "Target model present." }
  else { Warn "Target model '$Model' not in list. Run: ollama pull $Model" }
} catch { Warn "GET /api/tags failed: $($_.Exception.Message)" }

Step "Smoke test generate (short)..."
try {
  $body = @{ model = $Model; prompt = "Say OK in one word."; stream = $false } | ConvertTo-Json -Depth 3
  $gen = Invoke-RestMethod -Uri "$OllamaUrl/api/generate" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 180
  Write-Host ("  Response: " + $gen.response) -ForegroundColor Green
  Ok "Generate works!"
} catch { Warn "Generate test failed (model may still be loading): $($_.Exception.Message)" }

Write-Host "`nDone! Ollama should now be RUNNING." -ForegroundColor Green
Write-Host "  API: $OllamaUrl  |  Test: curl $OllamaUrl/api/tags" -ForegroundColor Gray
Write-Host "  Your app's default VPS (46.202.170.203:11434) still works; to use LOCAL set OllamaBaseUrl=http://localhost:11434" -ForegroundColor DarkGray
Write-Host "  Useful: ollama list | ollama ps | ollama run $Model" -ForegroundColor DarkGray
if (-not $Yes) { Write-Host "`nPress Enter to close..." -ForegroundColor DarkGray; Read-Host | Out-Null }
