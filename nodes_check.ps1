$o = @()
$nodes = @('LoadImage','CheckpointLoaderSimple','VAEEncode','VAEDecode','KSampler','SaveImage','LoraZero','LoraLoader','LoraLoaderModelOnly','ConditioningZeroOut')
foreach ($n in $nodes) {
  $code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 5 "http://127.0.0.1:8188/object_info/$n"
  $o += ($n + ' http=' + $code)
}
# sampler + scheduler options from KSampler
$j = (& curl.exe -s --max-time 5 'http://127.0.0.1:8188/object_info/KSampler') -join ''
if ($j) {
  try {
    $k = $j | ConvertFrom-Json
    $o += ('sampler_name: ' + ($k.KSampler.input.required.sampler_name[0] -join ', '))
    $o += ('scheduler: ' + ($k.KSampler.input.required.scheduler[0] -join ', '))
  } catch { $o += 'kparse failed' }
}
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\nodes_19.txt'
Write-Output 'NODES DONE'
