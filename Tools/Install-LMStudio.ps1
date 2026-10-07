#Requires -Version 5.1
<#
.SYNOPSIS
  One-click installer + launcher for LM Studio (Windows).
.DESCRIPTION
  1. Installs LM Studio via winget (ID: ElementLabs.LMStudio) if not present.
  2. Verifies install, launches the app, optionally starts local server.
  3. Works when double-clicked (Run with PowerShell) or run from terminal.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Install-LMStudio.ps1
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Install-LMStudio.ps1 -StartServer -Model "qwen2.5-vl-3b-instruct"
#>
param(
  [switch]$StartServer,
  [string]$Model = "",
  [switch]$NoLaunch,
  [switch]$Yes
)

$ErrorActionPreference = "Stop"
$PackageId = "ElementLabs.LMStudio"
$ExeCandidates = @(
  "$env:LOCALAPPDATA\LM Studio\LM Studio.exe",
  "$env:ProgramFiles\LM Studio\LM Studio.exe",
  "${env:ProgramFiles(x86)}\LM Studio\LM Studio.exe"
)

function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg) { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  [!!] $msg" -ForegroundColor Yellow }

function Find-LMStudio {
  foreach ($p in $ExeCandidates) {
    if (Test-Path $p) { return $p }
  }
  # StartMenu shortcut fallback
  $shortcut = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\LM Studio.lnk"
  if (Test-Path $shortcut) { return $shortcut }
  # PATH fallback (lms CLI ships with LM Studio)
  $lms = Get-Command lms -ErrorAction SilentlyContinue
  if ($lms) { return $lms.Source }
  return $null
}

function Test-Winget {
  try { winget --version | Out-Null; return $true } catch { return $false }
}

Write-Host "==============================================" -ForegroundColor Magenta
Write-Host " LM Studio One-Click Installer (TubeMailGorilla)" -ForegroundColor Magenta
Write-Host "==============================================" -ForegroundColor Magenta

$found = Find-LMStudio
if ($found) {
  Write-Ok "LM Studio already found at: $found"
} else {
  Write-Step "LM Studio not found. Installing via winget ($PackageId)..."

  if (-not (Test-Winget)) {
    Write-Warn "winget not found. Opening official download page instead..."
    Start-Process "https://lmstudio.ai/download"
    Write-Host "Download 'LM Studio for Windows' and run the installer, then re-run this script." -ForegroundColor Yellow
    if (-not $Yes) { Read-Host "Press Enter to exit" | Out-Null }
    exit 1
  }

  # winget install (interactive progress UI - do NOT use --silent here so user sees progress)
  $args = @("install", "--id=$PackageId", "-e", "--accept-source-agreements", "--accept-package-agreements")
  Write-Host "Running: winget $($args -join ' ')" -ForegroundColor Gray
  try {
    winget @args
  } catch {
    Write-Warn "winget install exited non-zero. It may still have succeeded - re-checking..."
  }

  Start-Sleep -Seconds 3
  $found = Find-LMStudio
  if (-not $found) {
    # Second chance: winget list check
    $list = winget list --name "LM Studio" 2>&1 | Out-String
    Write-Host $list
    if ($list -match "LM Studio|ElementLabs") {
      Write-Ok "winget reports LM Studio installed, but exe path not in known list. Trying Start Menu..."
      $found = "LM Studio (Start Menu)"
    } else {
      Write-Warn "Install not detected. Opening official download page as fallback..."
      Start-Process "https://lmstudio.ai/download"
      Write-Host "Install manually, then re-run this script." -ForegroundColor Yellow
      if (-not $Yes) { Read-Host "Press Enter to exit" | Out-Null }
      exit 1
    }
  }
  Write-Ok "Installed: $found"
}

# Verify
Write-Step "Verifying install..."
winget list --name "LM Studio"
try { lms --version } catch { Write-Warn "'lms' CLI not on PATH yet (normal until reboot/relogin)." }

# Launch
if (-not $NoLaunch) {
  Write-Step "Launching LM Studio..."
  $exe = Find-LMStudio
  try {
    if ($exe -like "*.lnk") {
      Start-Process $exe
    } elseif ($exe -like "*lms*") {
      Start-Process "lmstudio" -ErrorAction SilentlyContinue
      if (-not $?) { Start-Process "https://lmstudio.ai/download" }
    } else {
      Start-Process $exe
    }
    Write-Ok "Launch command sent. Look for LM Studio window / tray icon."
  } catch {
    Write-Warn "Auto-launch failed: $($_.Exception.Message)"
    Write-Host "Press Win key, type 'LM Studio', Enter." -ForegroundColor Yellow
  }
}

# Optional: start local OpenAI-compatible server
if ($StartServer) {
  Write-Step "Starting LM Studio local server (http://localhost:1234)..."
  try {
    if ([string]::IsNullOrWhiteSpace($Model)) {
      lms server start --port 1234
    } else {
      lms load $Model 2>&1 | Out-Null
      lms server start --port 1234
    }
    Write-Ok "Server starting. Test with: curl http://localhost:1234/v1/models"
  } catch {
    Write-Warn "Could not auto-start server: $($_.Exception.Message)"
    Write-Host "In LM Studio: Developer tab -> Start Server (port 1234)." -ForegroundColor Yellow
  }
} else {
  Write-Host "`nTip: to also auto-start the API server next time, run:" -ForegroundColor DarkGray
  Write-Host "  powershell -ExecutionPolicy Bypass -File `"$PSCommandPath`" -StartServer" -ForegroundColor DarkGray
}

Write-Host "`nDone! LM Studio should now be open." -ForegroundColor Green
Write-Host "Quick test once server is on:  curl http://localhost:1234/v1/models" -ForegroundColor Gray
if (-not $Yes) {
  # Only pause when double-clicked (no interactive console owner would close instantly)
  if ([Environment]::UserInteractive -and -not [Environment]::GetCommandLineArgs().Contains("-NoExit")) {
    # Keep window open briefly when double-clicked so user sees result
    Write-Host "`nPress Enter to close..." -ForegroundColor DarkGray
    Read-Host | Out-Null
  }
}
