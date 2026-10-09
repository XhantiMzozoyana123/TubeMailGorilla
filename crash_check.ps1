$r = @()
# --- ComfyUI boot status ---
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/system_stats
$r += ('comfyui http=' + $code)

# --- why did TubeMailGorilla.Maui exit? look for crash entries ---
$since = (Get-Date).AddMinutes(-15)
$ev = Get-WinEvent -FilterHashtable @{LogName='Application'; StartTime=$since} -ErrorAction SilentlyContinue |
      Where-Object { $_.Message -like '*TubeMailGorilla.Maui*' -and $_.Message -notlike '*Unlocked*' }
if ($ev) {
    foreach ($e in ($ev | Select-Object -First 4)) {
        $r += ('EVENT ' + $e.Id + ' [' + $e.TimeCreated.ToString('HH:mm:ss') + '] ' + ($e.Message -replace '\s+',' ').Substring(0, [Math]::Min(500, $e.Message.Length)))
    }
} else {
    $r += 'no crash events for TubeMailGorilla.Maui in last 15 min'
}
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\crash_40.txt' -Encoding utf8
Write-Output ($r -join ' || ')
