$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$bin = Join-Path $root 'TubeMailGorilla.Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$exe = Join-Path $bin 'TubeMailGorilla.Maui.exe'
$r = @()

$alive = Get-Process 'TubeMailGorilla.Maui' -ErrorAction SilentlyContinue
if (-not $alive) {
    $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
    $r += ('RETRY LAUNCHED pid=' + $p.Id)
    Start-Sleep -Seconds 12
    $p2 = Get-Process 'TubeMailGorilla.Maui' -ErrorAction SilentlyContinue
    if ($p2) { $r += ('ALIVE responding=' + $p2.Responding + ' window=[' + $p2.MainWindowTitle + ']') }
    else     { $r += 'DIED AGAIN within 12s' }
} else {
    $r += ('ALIVE already pid=' + $alive.Id + ' window=[' + $alive.MainWindowTitle + ']')
}

# full crash detail (all matching events incl. .NET Runtime 1026)
$since = (Get-Date).AddMinutes(-20)
$ev = Get-WinEvent -FilterHashtable @{LogName='Application'; StartTime=$since} -ErrorAction SilentlyContinue |
      Where-Object { $_.Message -like '*TubeMailGorilla.Maui*' -and $_.Message -notlike '*Unlocked*' }
foreach ($e in ($ev | Select-Object -First 6)) {
    $r += '---- EVENT ' + $e.Id + ' ' + $e.TimeCreated.ToString('HH:mm:ss')
    $r += ($e.Message -replace '\s+', ' ')
}
$r | Set-Content (Join-Path $root 'retry_41.txt') -Encoding utf8
Write-Output ($r[0..1] -join ' | ')
