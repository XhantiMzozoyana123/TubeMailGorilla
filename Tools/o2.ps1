$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\o2.txt'
$lines = @('start=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

$img = [Convert]::ToBase64String([IO.File]::ReadAllBytes('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'))
$t = 'Tonight we are touring this five bedroom house in Orlando with a full walkthrough of the kitchen and the primary suite and the backyard. '
$transcript = $t * 6
$reviewPrompt = 'Review this YouTube video and give the editor specific changes that would raise watch time, retention and engagement.' + "`n" +
    'Video details:' + "`n" +
    '- Title: Our Biggest Home Tour Yet!' + "`n" +
    '- Channel: Mark Callender Homes' + "`n" +
    '- Length: 12m 40s' + "`n" +
    '- Description: Walk through of a USD 1.2M custom home in Orlando. Full tour with commentary on the build quality, layout decisions and staging.' + "`n`n" +
    'Transcript:' + "`n" + $transcript + "`n`n" +
    'Sample frames from across the video are attached. Comment on what you can actually see in them.' + "`n`n" +
    'Answer under these four headings, with at most 2 short points under each. Be brief - one sentence per point.' + "`n`n" +
    'HOOK - the opening line or first shot to change, and why viewers leave now.' + "`n" +
    'PACING - the one cut or reorder that would most improve retention.' + "`n" +
    'PACKAGING - the specific title and thumbnail change.' + "`n" +
    'CTA - the end screen or call to action to add.'
$sysAdv = 'You are a senior YouTube editor and retention strategist. You give specific, concrete, actionable editing advice about a video. Rules: Ground every point in the material you are given. Be specific and practical. Write plain text. No markdown headers, no bold/asterisks, no emoji. Keep it tight and skimmable.'
$lines += 'prompt_chars=' + $reviewPrompt.Length

$json = '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":1200,"num_ctx":8192},"messages":[{"role":"system","content":"' + $sysAdv.Replace('"', '\"') + '"},{"role":"user","content":"' + $reviewPrompt.Replace('"', '\"').Replace("`r`n", '\n').Replace("`n", '\n') + '","images":["' + $img + '","' + $img + '","' + $img + '","' + $img + '"]}]}'
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
    if ($contentLen -eq 0) { $lines += 'CONTENT EMPTY!' } else { $lines += 'content_head=' + $resp.message.content.Substring(0, [Math]::Min(250, $contentLen)) }
    if ($resp.error) { $lines += 'ERR=' + $resp.error }
} catch {
    $lines += 'PARSE FAIL: ' + $_.Exception.Message + ' head=' + $text.Substring(0, [Math]::Min(200, $text.Length))
}
$lines | Set-Content $out -Encoding utf8
