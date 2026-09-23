# =====================================================================
# TubeMailGorilla - One-command Windows installer build (Inno Setup)
#
#   powershell -ExecutionPolicy Bypass -File Installer\build-installer.ps1
#
# Optional parameters:
#   -Version 1.1.0        installer / bundle version (default 1.1.0)
#   -SkipPublish          reuse an existing publish output
#   -SkipWebCopy          do not copy the distribution zip to TubeMailGorilla.Web
#
# Outputs:
#   Installer\TubeMailGorilla-setup.exe            (the installer)
#   Installer\TubeMailGorilla-v<Version>-Windows.zip (installer + install guide)
#   TubeMailGorilla.Web\public\downloads\TubeMailGorilla-v<Version>-Windows.zip
#       (served by the website's "Download for Windows" button)
#
# Prerequisites (auto-checked):
#   - .NET SDK with the MAUI workload
#   - Inno Setup 6.3 or newer  ->  winget install JRSoftware.InnoSetup
# =====================================================================
param(
    [string]$Version = "1.1.0",
    [switch]$SkipPublish,
    [switch]$SkipWebCopy
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent      # TubeMailGorilla.Maui folder
$Repo = Split-Path $Root -Parent              # repo root

# Locate Inno Setup compiler (ISCC.exe)
$IsccCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe"
)
$Iscc = $IsccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $Iscc) {
    $Iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
}
if (-not $Iscc) {
    Write-Error "Inno Setup 6 not found. Install it with: winget install JRSoftware.InnoSetup"
}

# 1. Publish the MAUI app (self-contained win-x64).
#    The .gguf AI model is intentionally NOT packed into the installer -
#    the Inno Setup wizard downloads it from Hugging Face during
#    installation, so end users never run any technical steps.
$PublishDir = Join-Path $Root "bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"
if (-not $SkipPublish) {
    Write-Host "==> [1/4] Publishing MAUI app (self-contained win-x64)..." -ForegroundColor Cyan
    dotnet publish (Join-Path $Root "TubeMailGorilla.Maui.csproj") `
        -c Release -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained `
        -p:PublishReadyToRun=true
    if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed." }
} else {
    Write-Host "==> [1/4] Skipping publish (reusing existing output)" -ForegroundColor Cyan
}
if (-not (Test-Path (Join-Path $PublishDir "TubeMailGorilla.Maui.exe"))) {
    Write-Error "Publish output not found at $PublishDir. Run without -SkipPublish."
}

# 2. Compile the installer with Inno Setup
Write-Host "==> [2/4] Compiling installer with Inno Setup..." -ForegroundColor Cyan
& $Iscc "/DMyAppVersion=$Version" (Join-Path $PSScriptRoot "TubeMailGorilla.iss")
if ($LASTEXITCODE -ne 0) { Write-Error "Inno Setup compilation failed." }

$setup = Get-Item (Join-Path $PSScriptRoot "TubeMailGorilla-setup.exe")

# 3. Package the distribution bundle handed out from the website:
#    TubeMailGorilla-v<Version>-Windows.zip  ->  setup.exe + install guide
Write-Host "==> [3/4] Packaging the distribution zip..." -ForegroundColor Cyan
$Guide = Join-Path $PSScriptRoot "Download\INSTALL-GUIDE.txt"
if (-not (Test-Path $Guide)) { Write-Error "Install guide not found at $Guide" }
$ZipName = "TubeMailGorilla-v$Version-Windows.zip"
$ZipPath = Join-Path $PSScriptRoot $ZipName
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
Compress-Archive -Path $setup.FullName, $Guide -DestinationPath $ZipPath -CompressionLevel Optimal
$Zip = Get-Item $ZipPath

# 4. Publish the bundle to the website's public download folder, replacing
#    any older versioned bundles so the deployment does not grow forever.
$WebDownloads = Join-Path $Repo "TubeMailGorilla.Web\public\downloads"
if ($SkipWebCopy) {
    Write-Host "==> [4/4] Skipping website copy (-SkipWebCopy)" -ForegroundColor Cyan
} elseif (Test-Path $WebDownloads) {
    Write-Host "==> [4/4] Copying the bundle to the website downloads folder..." -ForegroundColor Cyan
    Get-ChildItem $WebDownloads -Filter "TubeMailGorilla-v*-Windows.zip" |
        Where-Object { $_.Name -ne $ZipName } |
        ForEach-Object { Write-Host ("    removing old bundle {0}" -f $_.Name); Remove-Item $_.FullName -Force }
    Copy-Item $ZipPath (Join-Path $WebDownloads $ZipName) -Force
} else {
    Write-Host "==> [4/4] Website downloads folder not found - skipping copy" -ForegroundColor Yellow
}

Write-Host ""
Write-Host ("Done!  {0}  ({1:N0} MB)" -f $setup.FullName, ($setup.Length/1MB)) -ForegroundColor Green
Write-Host ("Done!  {0}  ({1:N0} MB)" -f $Zip.FullName, ($Zip.Length/1MB)) -ForegroundColor Green
Write-Host "The setup wizard downloads the AI model (~1.9 GB) during installation." -ForegroundColor Green
