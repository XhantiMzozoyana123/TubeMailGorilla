$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()

# --- preflight: services the apps use ---
$r += ('comfyui http=' + (& curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats))
$r += ('ollama http=' + (& curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://localhost:11434/api/tags))

$apps = @(
    @{ name = 'TubeMailGorilla.Maui';          dir = 'TubeMailGorilla.Maui' },
    @{ name = 'TubeMailGorilla.Maui.Unlocked'; dir = 'TubeMailGorilla.Maui.Unlocked' }
)
foreach ($a in $apps) {
    $existing = Get-Process $a.name -ErrorAction SilentlyContinue
    if ($existing) {
        $r += ($a.name + ' ALREADY RUNNING pid=' + $existing.Id)
        continue
    }
    $bin = Join-Path $root ($a.dir + '\bin\Debug\net10.0-windows10.0.19041.0\win-x64')
    $exe = Join-Path $bin ($a.name + '.exe')
    if (-not (Test-Path $exe)) { $r += ($a.name + ' MISSING exe: ' + $exe); continue }
    # both projects copy appsettings.json to output (PreserveNewest)
    $cfg = Join-Path $bin 'appsettings.json'
    $hasComfy = $false
    if (Test-Path $cfg) { $hasComfy = (Get-Content $cfg -Raw) -like '*ComfyUiBaseUrl*' }
    $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
    $r += ($a.name + ' LAUNCHED pid=' + $p.Id + ' cfgHasComfy=' + $hasComfy)
}
$r | Set-Content (Join-Path $root 'launchboth_38.txt') -Encoding utf8
Write-Output ($r -join ' | ')
