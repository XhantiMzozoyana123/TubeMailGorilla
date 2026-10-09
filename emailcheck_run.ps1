$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$out = Join-Path $root 'emailcheck_31.txt'
if (Test-Path $out) { Remove-Item $out -Force }
# The app assembly's WindowsAppSDK module initializer P/Invokes
# Microsoft.WindowsAppRuntime.dll; native probing also searches PATH, so expose
# the app's output folder to the harness process.
$appBin = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$env:PATH = $appBin + ';' + $env:PATH
$p = Start-Process -FilePath 'dotnet' `
    -ArgumentList 'run', '--project', (Join-Path $root 'Tools\EmailHtmlCheck'), '-v', 'q' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $out -RedirectStandardError ($out + '.err')
$p.Id | Set-Content (Join-Path $root 'emailcheck.pid')
Write-Output ("CHECK LAUNCHED pid=" + $p.Id)
