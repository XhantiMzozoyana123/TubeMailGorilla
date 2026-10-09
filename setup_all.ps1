$ErrorActionPreference = 'Continue'
$Base   = 'C:\Users\Shadow\ComfyUI'
$Dl     = Join-Path $Base 'dl'
$Log    = Join-Path $Base 'setup.log'
$Portable = Join-Path $Dl 'ComfyUI_windows_portable_nvidia.7z'
$Model    = Join-Path $Dl 'v1-5-pruned-emaonly.safetensors'
$SevenZr   = Join-Path $Dl '7zr.exe'

function Say($m){ $line = ("[{0}] {1}" -f (Get-Date -Format 'HH:mm:ss'), $m); Add-Content $Log $line }
function Dl($url,$dest){ & curl.exe -L -C - --retry 5 --retry-delay 3 -o $dest $url; return $LASTEXITCODE }

New-Item -ItemType Directory -Force -Path $Dl | Out-Null
Set-Content $Log 'SETUP START'

# 1) 7zr extractor (small)
Say 'STEP1 downloading 7zr.exe'
Dl 'https://www.7-zip.org/a/7zr.exe' $SevenZr | Out-Null
if (-not (Test-Path $SevenZr)) { Say 'STEP1 FAILED 7zr missing'; exit 1 }
Say 'STEP1 ok'

# 2) ComfyUI portable (~2.5GB) - resume-capable
Say 'STEP2 downloading ComfyUI portable (this is large)'
$code = Dl 'https://github.com/comfyanonymous/ComfyUI/releases/latest/download/ComfyUI_windows_portable_nvidia.7z' $Portable
$sz = if (Test-Path $Portable) { (Get-Item $Portable).Length } else { 0 }
Say ("STEP2 done code={0} size={1}MB" -f $code, [math]::Round($sz/1MB,1))
if ($code -ne 0 -or $sz -lt 100MB) { Say 'STEP2 FAILED'; exit 2 }

# 3) Extract
Say 'STEP3 extracting (cpu heavy, be patient)'
& $SevenZr x $Portable ("-o" + $Base) -y | Out-Null
$extracted = Get-ChildItem $Base -Directory -Filter 'ComfyUI_windows_portable*' | Select-Object -First 1
if (-not $extracted) { Say 'STEP3 FAILED no portable dir'; exit 3 }
Say ("STEP3 ok -> {0}" -f $extracted.FullName)

# 4) SD1.5 checkpoint (~4GB)
Say 'STEP4 downloading SD1.5 checkpoint'
$ckptDir = Join-Path $extracted.FullName 'ComfyUI\models\checkpoints'
New-Item -ItemType Directory -Force -Path $ckptDir | Out-Null
$code = Dl 'https://huggingface.co/stable-diffusion-v1-5/stable-diffusion-v1-5/resolve/main/v1-5-pruned-emaonly.safetensors' $Model
$msz = if (Test-Path $Model) { (Get-Item $Model).Length } else { 0 }
Say ("STEP4 done code={0} size={1}MB" -f $code, [math]::Round($msz/1MB,1))
if ($code -ne 0 -or $msz -lt 1GB) { Say 'STEP4 FAILED'; exit 4 }
Copy-Item $Model (Join-Path $ckptDir 'v1-5-pruned-emaonly.safetensors') -Force
Say 'STEP4 model placed in checkpoints'

Say 'SETUP COMPLETE'
Write-Output 'SETUP SCRIPT FINISHED'
