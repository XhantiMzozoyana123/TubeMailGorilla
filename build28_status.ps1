$r = @()
$running = Get-Process -Id 20084 -ErrorAction SilentlyContinue
$r += ('build running = ' + ($null -ne $running))
$log = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build_28.log' -ErrorAction SilentlyContinue
if ($log) {
    $hits = $log | Select-String 'Error\(s\)|Warning\(s\)|error CS'
    foreach ($h in $hits) { $r += $h.Line }
} else { $r += 'log missing' }
# also confirm the bin appsettings got the new model
$bin = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64\appsettings.json'
if (Test-Path $bin) {
    $cfg = Get-Content $bin -Raw
    $r += ('bin appsettings llama3 = ' + ($cfg -like '*llama3:latest*'))
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\build28_status_55.txt' -Encoding utf8
Write-Output ($r -join ' | ')
