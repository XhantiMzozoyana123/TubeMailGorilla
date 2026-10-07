<#
.SYNOPSIS
    Fetches the local vision model for the Unlocked MAUI app.

.DESCRIPTION
    llama.cpp cannot bolt a vision projector onto an arbitrary chat model: the
    mmproj file is trained against ONE base architecture and its output feeds
    that model's embedding table. Pairing a Qwen2.5-VL projector with
    Llama-3.2 weights fails to load, so this app uses a single vision-language
    model for BOTH jobs - text extraction and the [snapshot_ai] image pick.

    That means two files, which must come from the SAME repo and quant family:

      Qwen2.5-VL-3B-Instruct-Q4_K_M.gguf     ~1.9 GB  the LLM (text + vision)
      mmproj-Qwen2.5-VL-3B-Instruct-Q8_0.gguf ~845 MB the vision projector

    ~2.8 GB total, which offloads fully to an RTX 2000 Ada (16 GB) and also
    runs on a much smaller card. Both are Apache-2.0 on Hugging Face, from the
    ggml-org account that llama.cpp itself documents for -hf / --mmproj.

    Files land in the app's default model directory so a normal install needs
    no configuration. Re-running only fetches what is missing.

.PARAMETER Project
    Which app to fetch for. The Unlocked edition is the default.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File Tools\download-vision-model.ps1
#>
param(
    [ValidateSet('Unlocked', 'Maui')]
    [string]$Project = 'Unlocked',

    [string]$ModelDirectory = '',
    [string]$BaseUrl = 'https://huggingface.co/ggml-org/Qwen2.5-VL-3B-Instruct-GGUF/resolve/main'
)

$ErrorActionPreference = 'Stop'

$files = @(
    'Qwen2.5-VL-3B-Instruct-Q4_K_M.gguf',
    'mmproj-Qwen2.5-VL-3B-Instruct-Q8_0.gguf'
)

if (-not $ModelDirectory) {
    # Must match LLMService.ModelDirectory, which falls back to this same path.
    $ModelDirectory = Join-Path $env:LOCALAPPDATA 'TubeMailGorillaUnlocked\models'
}
$ModelDirectory = [System.IO.Path]::GetFullPath($ModelDirectory)
New-Item -ItemType Directory -Force -Path $ModelDirectory | Out-Null

Write-Host "Project     : $Project"
Write-Host "Model folder: $ModelDirectory"
Write-Host ''

foreach ($name in $files) {
    $out = Join-Path $ModelDirectory $name

    if ((Test-Path $out) -and (Get-Item $out).Length -gt 0) {
        Write-Host ("Skipping {0} - already present ({1:N1} MB)" -f $name, ((Get-Item $out).Length / 1MB))
        continue
    }

    $url = "$BaseUrl/$name"
    $part = "$out.part"

    Write-Host "Downloading $name ..."
    $client = New-Object System.Net.WebClient
    $client.Headers.Add('User-Agent', 'TubeMailGorilla/1.0')
    try {
        $client.DownloadFile($url, $part)
    }
    catch {
        $client.Dispose()
        Remove-Item $part -Force -ErrorAction SilentlyContinue
        throw "Failed to download $name from $url : $($_.Exception.Message)"
    }
    $client.Dispose()

    if (Test-Path $out) { Remove-Item $out -Force }
    Move-Item $part $out -Force
    Write-Host ("Done: {0} ({1:N1} MB)" -f $out, ((Get-Item $out).Length / 1MB))
}

Write-Host ''
Write-Host 'Now set these in TubeMailGorilla.Maui.Unlocked\appsettings.json:'
Write-Host '  "ChatModelPath"   -> the Qwen2.5-VL-3B gguf'
Write-Host '  "VisionModelPath" -> the mmproj gguf'