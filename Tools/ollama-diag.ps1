$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\ollama-diag.txt'
$lines = @()
$lines += 'ollama_proc=' + (@(Get-Process ollama -ErrorAction SilentlyContinue).Count)
$listeners = Get-NetTCPConnection -LocalPort 11434 -State Listen -ErrorAction SilentlyContinue
foreach ($l in $listeners) {
    $proc = Get-Process -Id $l.OwningProcess -ErrorAction SilentlyContinue
    $lines += 'listener=' + $l.LocalAddress + ':' + $l.LocalPort + ' pid=' + $l.OwningProcess + ' proc=' + $proc.Name
}
try {
    $raw = Invoke-WebRequest -Uri 'http://localhost:11434/api/tags' -UseBasicParsing -TimeoutSec 10
    $lines += 'tags_status=' + $raw.StatusCode
    $lines += 'tags_body=' + $raw.Content.Substring(0, [Math]::Min(600, $raw.Content.Length))
} catch { $lines += 'tags_ERR=' + $_.Exception.Message }
try {
    $v = Invoke-WebRequest -Uri 'http://localhost:11434/api/version' -UseBasicParsing -TimeoutSec 10
    $lines += 'version_body=' + $v.Content
} catch { $lines += 'version_ERR=' + $_.Exception.Message }
try {
    $r = Invoke-WebRequest -Uri 'http://localhost:11434/' -UseBasicParsing -TimeoutSec 10
    $lines += 'root_status=' + $r.StatusCode + ' body=' + $r.Content.Substring(0, [Math]::Min(200, $r.Content.Length))
} catch { $lines += 'root_ERR=' + $_.Exception.Message }
#ollama list via CLI
try {
    $cli = & "$env:LOCALAPPDATA\Programs\Ollama\ollama.exe" list 2>&1
    $lines += 'ollama_list=' + ($cli -join ' | ')
} catch { $lines += 'ollama_list_ERR=' + $_.Exception.Message }
$lines | Set-Content $out
