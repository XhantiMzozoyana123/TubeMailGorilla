# =====================================================================
# TubeMailGorilla - One-command Windows installer build
#
#   powershell -ExecutionPolicy Bypass -File Installer\build-installer.ps1
#
# Optional parameters:
#   -Version 1.0.0        installer version
#   -SkipPublish          reuse an existing publish output
#
# Prerequisites (auto-checked):
#   - .NET SDK with MAUI workload
#   - WiX Toolset v7 installed at 'C:\Program Files\WiX Toolset v7.0'
# =====================================================================
param(
    [string]$Version = "1.0.0",
    [switch]$SkipPublish
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent   # TubeMailGorilla.Maui folder
$Repo = Split-Path $Root -Parent           # repo root
$Wix  = "$env:ProgramFiles\WiX Toolset v7.0\bin\wix.exe"

if (-not (Test-Path $Wix)) {
    Write-Error "WiX Toolset v7 not found at $Wix. Install with: winget install WiXToolset.WiXCLI"
}

# 1. Publish the MAUI app (self-contained win-x64; the .gguf model is
#    copied here if present locally, but is NOT packed into the MSI -
#    it is downloaded from HuggingFace during installation instead).
$PublishDir = Join-Path $Root "bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"
if (-not $SkipPublish) {
    Write-Host "==> [1/4] Publishing MAUI app..." -ForegroundColor Cyan
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

# 2. Build the custom action (downloads the LLM model during install)
Write-Host "==> [2/4] Building model-download custom action..." -ForegroundColor Cyan
dotnet build (Join-Path $Repo "TubeMailGorilla.CustomActions\TubeMailGorilla.CustomActions.csproj") -c Release
if ($LASTEXITCODE -ne 0) { Write-Error "Custom action build failed." }

# 3. Harvest files + build MSI
Write-Host "==> [3/4] Building MSI (this takes a few minutes)..." -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "generate-harvest.ps1") `
    -PublishDir $PublishDir `
    -OutFile (Join-Path $PSScriptRoot "harvested.wxs")

& $Wix build -arch x64 -acceptEula wix7 `
    -o (Join-Path $PSScriptRoot "TubeMailGorilla.msi") `
    (Join-Path $PSScriptRoot "harvested.wxs")
if ($LASTEXITCODE -ne 0) { Write-Error "MSI build failed." }

# 4. Wrap MSI into setup.exe bootstrapper
Write-Host "==> [4/4] Building setup.exe..." -ForegroundColor Cyan
$ext = Join-Path $PSScriptRoot "tools\WixToolset.BootstrapperApplications.wixext.dll"
if (-not (Test-Path $ext)) {
    $ext = Get-ChildItem "$env:ProgramFiles\WiX Toolset v7.0" -Filter "WixToolset.BootstrapperApplications.wixext.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
}
$extArgs = @()
if ($ext -and (Test-Path $ext)) { $extArgs = @("-ext", $ext) }

& $Wix build -arch x64 -acceptEula wix7 @extArgs -bindpath $PSScriptRoot `
    -o (Join-Path $PSScriptRoot "TubeMailGorilla-setup.exe") `
    (Join-Path $PSScriptRoot "Bundle.wxs")
if ($LASTEXITCODE -ne 0) { Write-Error "setup.exe build failed." }

$msi  = Get-Item (Join-Path $PSScriptRoot "TubeMailGorilla.msi")
$exe  = Get-Item (Join-Path $PSScriptRoot "TubeMailGorilla-setup.exe")
Write-Host ""
Write-Host ("Done!  {0}  ({1:N0} MB)" -f $exe.FullName, ($exe.Length/1MB)) -ForegroundColor Green
Write-Host ("       {0}  ({1:N0} MB)" -f $msi.FullName, ($msi.Length/1MB)) -ForegroundColor Green
