$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$proj = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\TubeMailGorilla.Maui.Unlocked.csproj'
$out  = Join-Path $root 'build_out_25.txt'
if (Test-Path $out) { Remove-Item $out -Force }
$log  = Join-Path $root 'build_25.log'
if (Test-Path $log) { Remove-Item $log -Force }
$p = Start-Process -FilePath 'dotnet' -ArgumentList 'build', $proj, '-v', 'm', '-nologo' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
$p.Id | Set-Content (Join-Path $root 'build_25.pid')
Write-Output ("BUILD LAUNCHED pid=" + $p.Id)
