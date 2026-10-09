# Stop the running app, build the token-chip changes, report errors.
$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$u = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($u) { Stop-Process -Id $u.Id -Force; Write-Output ('stopped app pid=' + $u.Id) }

$proj = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\TubeMailGorilla.Maui.Unlocked.csproj'
$log  = Join-Path $root 'build_29.log'
if (Test-Path $log) { Remove-Item $log -Force }
$p = Start-Process -FilePath 'dotnet' -ArgumentList 'build', $proj, '-v', 'm', '-nologo' `
    -WorkingDirectory $root -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
$p.Id | Set-Content (Join-Path $root 'build_29.pid')
Write-Output ("BUILD LAUNCHED pid=" + $p.Id)
