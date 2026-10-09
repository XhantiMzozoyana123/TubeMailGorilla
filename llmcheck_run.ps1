$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$out = Join-Path $root 'llmcheck_result.txt'
if (Test-Path $out) { Remove-Item $out -Force }
# Same WindowsAppRuntime P/Invoke workaround as emailcheck_run.ps1: the app
# assembly's module initializer probes PATH for Microsoft.WindowsAppRuntime.dll.
$appBin = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$env:PATH = $appBin + ';' + $env:PATH
$p = Start-Process -FilePath 'dotnet' `
    -ArgumentList 'run', '--project', (Join-Path $root 'Tools\LLMCheck'), '-v', 'q' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $out -RedirectStandardError ($out + '.err')
$p.Id | Set-Content (Join-Path $root 'llmcheck.pid')
Write-Output ("LLMCHECK LAUNCHED pid=" + $p.Id)
