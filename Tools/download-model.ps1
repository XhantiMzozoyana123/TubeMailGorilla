param(
    [string]$Url = "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf",
    [string]$OutFile = ""
)

if (-not $OutFile) {
    $OutFile = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot "..\TubeMailGorilla.Maui\Resources\Models\Llama-3.2-3B-Instruct-Q4_K_M.gguf"))
}
$OutFile = [System.IO.Path]::GetFullPath($OutFile)
$dir = Split-Path $OutFile -Parent
New-Item -ItemType Directory -Force -Path $dir | Out-Null

$part = "$OutFile.part"
if (-not (Test-Path $OutFile)) { $KeepPartial = $true }

$client = New-Object System.Net.WebClient
$client.Headers.Add("User-Agent", "TubeMailGorilla.Maui/1.0")
Write-Host "Downloading model to: $OutFile"

try {
    $client.DownloadFile($Url, $part)
} finally {
    $client.Dispose()
}

if (Test-Path $OutFile) { Remove-Item $OutFile -Force }
Move-Item $part $OutFile -Force
Write-Host ("Done: {0} ({1:N1} MB)" -f $OutFile, ((Get-Item $OutFile).Length / 1MB))