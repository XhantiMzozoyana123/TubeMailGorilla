$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\n-only.txt'
$lines = @('start=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

$img = [Convert]::ToBase64String([IO.File]::ReadAllBytes('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'))
$bigPrompt = 'Review this YouTube video and give the editor specific changes that would raise watch time. HOOK - the opening line to change. PACING - the one cut. PACKAGING - the title and thumbnail change. CTA - the end screen to add.'
$json = '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":260,"num_ctx":8192},"messages":[{"role":"system","content":"You are a senior YouTube editor."},{"role":"user","content":"' + $bigPrompt + '","images":["' + $img + '","' + $img + '","' + $img + '","' + $img + '"]}]}'

$tmp = [IO.Path]::GetTempFileName()
[IO.File]::WriteAllText($tmp, $json)
$raw = & curl.exe -s --max-time 180 -X POST 'http://localhost:11434/api/chat' -H 'Content-Type: application/json' -d "@$tmp"
Remove-Item $tmp -Force
$text = ($raw -join '')
$lines += 'raw_len=' + $text.Length
try {
    $resp = $text | ConvertFrom-Json
    $think = $resp.message.thinking
    if (-not $think) { $think = $resp.message.reasoning }
    $thinkLen = if ($think) { $think.Length } else { 0 }
    $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
    $lines += ('done={0} content_len={1} think_len={2}' -f $resp.done_reason, $contentLen, $thinkLen)
    if ($contentLen -eq 0) { $lines += 'CONTENT EMPTY!' } else { $lines += 'content_head=' + $resp.message.content.Substring(0, [Math]::Min(200, $contentLen)) }
    if ($resp.error) { $lines += 'ERR=' + $resp.error }
} catch {
    $lines += 'PARSE FAIL: ' + $_.Exception.Message
    $lines += 'raw_head=' + $text.Substring(0, [Math]::Min(300, $text.Length))
}
$lines | Set-Content $out -Encoding utf8
