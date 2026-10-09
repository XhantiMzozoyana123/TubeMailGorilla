$ck = 'C:\Users\Shadow\ComfyUI\ComfyUI_windows_portable\ComfyUI\models\checkpoints'
$o = @(Get-ChildItem $ck -ErrorAction SilentlyContinue | ForEach-Object { $_.Name + '  ' + [math]::Round($_.Length/1GB,2) + ' GB' })
$o += '--- object_info CheckpointLoaderSimple ---'
$code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 http://127.0.0.1:8188/object_info/CheckpointLoaderSimple
$o += ('http=' + $code)
if ($code -eq '200') {
  $j = (& curl.exe -s --max-time 5 http://127.0.0.1:8188/object_info/CheckpointLoaderSimple) -join ''
  try {
    $obj = $j | ConvertFrom-Json
    $o += ('models: ' + ($obj.CheckpointLoaderSimple.input.required.ckpt_name[0] -join ', '))
  } catch { $o += ('raw head: ' + $j.Substring(0, [Math]::Min(400, $j.Length))) }
}
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\ckpt_check_17.txt'
Write-Output 'CKPT DONE'
