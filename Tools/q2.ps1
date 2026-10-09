$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\q2.txt'
$lines = @('start=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

$sysExt = 'You extract contact fields from text. Reply with JSON only.'
$usrExt = 'Extract name and email from: "Contact John Smith at john.smith@example.com for the listing."'
$json = '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":600,"num_ctx":8192},"messages":[{"role":"system","content":"' + $sysExt.Replace('"', '\"') + '"},{"role":"user","content":"' + $usrExt.Replace('"', '\"') + '"}]}'
$tmp = [IO.Path]::GetTempFileName()
[IO.File]::WriteAllText($tmp, $json)
$raw = & curl.exe -s --max-time 240 -X POST 'http://localhost:11434/api/chat' -H 'Content-Type: application/json' -d "@$tmp"
Remove-Item $tmp -Force
$text = ($raw -join '')
try {
    $resp = $text | ConvertFrom-Json
    $think = $resp.message.thinking
    if (-not $think) { $think = $resp.message.reasoning }
    $thinkLen = if ($think) { $think.Length } else { 0 }
    $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
    $lines += ('done={0} content_len={1} think_len={2}' -f $resp.done_reason, $contentLen, $thinkLen)
    if ($contentLen -eq 0) { $lines += 'CONTENT EMPTY!' } else { $lines += 'content=' + $resp.message.content }
    if ($resp.error) { $lines += 'ERR=' + $resp.error }
} catch {
    $lines += 'PARSE FAIL: ' + $_.Exception.Message + ' head=' + $text.Substring(0, [Math]::Min(200, $text.Length))
}
$lines | Set-Content $out -Encoding utf8
