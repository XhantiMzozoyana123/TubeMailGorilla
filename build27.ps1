# Build (the app is not running - stopped before build26 and never relaunched).
$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$u = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($u) { Stop-Process -Id $u.Id -Force; Write-Output ('stopped app pid=' + $u.Id) }

$proj = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\TubeMailGorilla.Maui.Unlocked.csproj'
$log  = Join-Path $root 'build_27.log'
if (Test-Path $log) { Remove-Item $log -Force }
$p = Start-Process -FilePath 'dotnet' -ArgumentList 'build', $proj, '-v', 'm', '-nologo' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
$p.Id | Set-Content (Join-Path $root 'build_27.pid')
Write-Output ("BUILD LAUNCHED pid=" + $p.Id)
