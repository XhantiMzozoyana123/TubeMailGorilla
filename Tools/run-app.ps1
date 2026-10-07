$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\app-launch.txt'
$bin = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$exe = Join-Path $bin 'TubeMailGorilla.Maui.Unlocked.exe'
$lines = @('time=' + (Get-Date).ToString('HH:mm:ss'), 'exists=' + (Test-Path $exe), 'exe=' + $exe)
if (Test-Path $exe) {
    $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
    $lines += 'pid=' + $p.Id
}
$lines | Set-Content $out
