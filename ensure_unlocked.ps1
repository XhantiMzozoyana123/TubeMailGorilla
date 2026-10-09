$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$r = @()

# ---- 1) Ollama: up? required model present? ----
$tagsJson = & curl.exe -s --max-time 8 http://localhost:11434/api/tags
if ($LASTEXITCODE -eq 0 -and $tagsJson) {
    $r += 'ollama: UP'
    try {
        $models = ($tagsJson | ConvertFrom-Json).models | ForEach-Object { $_.name }
        $r += ('ollama models: ' + (($models | Sort-Object) -join ', '))
        $need = 'qwen3-vl:4b'   # LlmSettings.OllamaModel in Unlocked appsettings
        if ($models -contains $need) { $r += ('ollama: ' + $need + ' PRESENT') }
        else { $r += ('ollama: MISSING ' + $need) }
    } catch { $r += ('ollama tags parse failed: ' + $_.Exception.Message) }
} else {
    $r += 'ollama: DOWN'
}

# ---- 2) ComfyUI: up? checkpoint visible? ----
$c = & curl.exe -s -o NUL -w '%{http_code}' --max-time 8 http://127.0.0.1:8188/system_stats
$r += ('comfyui system_stats http=' + $c)
if ($c -eq '200') {
    $ck = & curl.exe -s --max-time 8 http://127.0.0.1:8188/models/checkpoints
    if ($ck -like '*v1-5-pruned-emaonly*') { $r += 'comfyui: checkpoint v1-5-pruned-emaonly PRESENT' }
    else { $r += ('comfyui: checkpoint list => ' + $ck.Substring(0, [Math]::Min(200, $ck.Length))) }
}

# ---- 3) user wants ONLY the Unlocked app: stop regular if my retry started it ----
$reg = Get-Process 'TubeMailGorilla.Maui' -ErrorAction SilentlyContinue
if ($reg) { Stop-Process -Id $reg.Id -Force; $r += ('regular app STOPPED (pid=' + $reg.Id + ')') }
else      { $r += 'regular app: not running' }

# ---- 4) ensure Unlocked is running ----
$u = Get-Process 'TubeMailGorilla.Maui.Unlocked' -ErrorAction SilentlyContinue
if ($u) {
    $r += ('Unlocked ALREADY RUNNING pid=' + $u.Id + ' window=[' + $u.MainWindowTitle + ']')
} else {
    $bin = Join-Path $root 'TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
    $exe = Join-Path $bin 'TubeMailGorilla.Maui.Unlocked.exe'
    if (-not (Test-Path $exe)) { $r += ('Unlocked exe MISSING: ' + $exe) }
    else {
        $p = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
        $r += ('Unlocked LAUNCHED pid=' + $p.Id)
    }
}
$r | Set-Content (Join-Path $root 'ensure_42.txt') -Encoding utf8
Write-Output ($r -join ' | ')
