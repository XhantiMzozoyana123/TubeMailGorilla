$o = @()
$o += '--- CLIPTextEncode ---'
$j1 = (& curl.exe -s --max-time 5 'http://127.0.0.1:8188/object_info/CLIPTextEncode') -join ''
$o += ('len=' + $j1.Length + ' head=' + $j1.Substring(0, [Math]::Min(300, $j1.Length)))
$o += '--- LoraZero body ---'
$j2 = (& curl.exe -s --max-time 5 'http://127.0.0.1:8188/object_info/LoraZero') -join ''
$o += ('len=' + $j2.Length + ' body=' + $j2.Substring(0, [Math]::Min(300, $j2.Length)))
$o += '--- LoraLoader body (known-good control) ---'
$j3 = (& curl.exe -s --max-time 5 'http://127.0.0.1:8188/object_info/LoraLoader') -join ''
$o += ('len=' + $j3.Length)
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\nodes2_20.txt'
Write-Output 'NODES2 DONE'
