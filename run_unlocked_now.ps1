$bin = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$exe = Join-Path $bin 'TubeMailGorilla.Maui.Unlocked.exe'
$outFile = 'c:\Users\Shadow\Documents\TubeMailGorilla\launch_result.txt'
$r = @()
$r += ('time=' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
$r += ('exe exists=' + (Test-Path $exe))
$existing = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($existing) {
    $r += ('STATUS=ALREADY_RUNNING pid=' + $existing.Id)
} else {
    try {
        $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
        Start-Sleep -Seconds 4
        $check = Get-Process -Id $p.Id -ErrorAction SilentlyContinue
        if ($check) { $r += ('STATUS=LAUNCHED pid=' + $p.Id + ' running=YES') }
        else { $r += ('STATUS=LAUNCHED_BUT_EXITED pid=' + $p.Id) }
    } catch {
        $r += ('STATUS=FAILED error=' + $_.Exception.Message)
    }
}
$r | Set-Content $outFile -Encoding utf8
