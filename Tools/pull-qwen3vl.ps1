$ErrorActionPreference = 'Continue'
$log     = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\pull-qwen3vl.log'
$ollama  = Join-Path $env:LOCALAPPDATA 'Programs\Ollama\ollama.exe'
"start $(Get-Date -Format o) exe=$ollama" | Set-Content $log
& $ollama pull qwen3-vl:4b *>> $log
"exit=$LASTEXITCODE end=$(Get-Date -Format o)" | Add-Content $log
