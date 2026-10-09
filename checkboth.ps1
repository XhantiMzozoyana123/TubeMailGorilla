$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()

# --- Unlocked app (may already be running from previous session) ---
$u = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($u) { $r += ('Unlocked ALREADY RUNNING pid=' + $u.Id + ' window=[' + $u.MainWindowTitle + ']') }
else    { $r += 'Unlocked NOT running' }

# --- Regular MAUI app: locate build output ---
$bin = Join-Path $root 'TubeMailGorilla.Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$exe = Join-Path $bin 'TubeMailGorilla.Maui.exe'
$r += ('regular exe exists = ' + (Test-Path $exe) + ' path=' + $exe)

$m = Get-Process 'TubeMailGorilla.Maui' -ErrorAction SilentlyContinue
if ($m) { $r += ('Regular ALREADY RUNNING pid=' + $m.Id + ' window=[' + $m.MainWindowTitle + ']') }
else    { $r += 'Regular NOT running' }

$r | Set-Content (Join-Path $root 'bothapps_37.txt') -Encoding utf8
Write-Output ($r -join ' | ')
