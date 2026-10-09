# Stop the running app (its exe/dlls are locked while it runs), then build.
$u = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($u) { Stop-Process -Id $u.Id -Force; Write-Output ('stopped app pid=' + $u.Id) }

$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$proj = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\TubeMailGorilla.Maui.Unlocked.csproj'
$log  = Join-Path $root 'build_26.log'
if (Test-Path $log) { Remove-Item $log -Force }
$p = Start-Process -FilePath 'dotnet' -ArgumentList 'build', $proj, '-v', 'm', '-nologo' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
$p.Id | Set-Content (Join-Path $root 'build_26.pid')
Write-Output ("BUILD LAUNCHED pid=" + $p.Id)
