$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$bin = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$exe = Join-Path $bin 'TubeMailGorilla.Maui.Unlocked.exe'
$log = Join-Path $root 'apprun_35.txt'
$r = @()
$r += ('exe exists = ' + (Test-Path $exe))

$existing = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($existing) {
    $r += ('ALREADY RUNNING pid=' + $existing.Id)
} else {
    # preflight: ComfyUI (app's AI features need it)
    $cc = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats
    $r += ('comfyui http=' + $cc)
    # self-contained + unpackaged (WindowsPackageType=None): launch exe directly,
    # working dir = app folder so appsettings.json/app-local files resolve.
    $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
    $r += ('LAUNCHED pid=' + $p.Id)
    $p.Id | Set-Content (Join-Path $root 'apppid.txt')
}
$r | Set-Content $log -Encoding utf8
Write-Output ($r -join ' | ')
