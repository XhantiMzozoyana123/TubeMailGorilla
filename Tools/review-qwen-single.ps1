$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\review-repro-qwen.txt'
$lines = @()
$lines += 'time=' + (Get-Date).ToString('HH:mm:ss')
$lines += 'model=qwen3-vl:4b num_predict=1500 num_ctx=8192 SINGLE-TEST'
$lines | Set-Content $out -Encoding utf8

$advSys = 'You are a senior YouTube editor and retention strategist. Give specific, concrete, actionable editing advice about a video. Ground every point in the material given. Be specific and practical. Plain text only.'

$transcript = ('Tonight we are touring this five bedroom house in Orlando with a full walkthrough of the kitchen and the primary suite and the backyard. ' * 6)
$prompt = "Review this YouTube video and give the editor specific changes that would raise watch time, retention and engagement.`n`nVideo details:`n- Title: Our Biggest Home Tour Yet!`n- Channel: Mark Callender Homes`n- Length: 12m 40s`n- Description: Walk through of a custom home in Orlando.`n`nTranscript:`n$transcript`n`nSample frames from across the video are attached. Comment on what you can actually see in them.`n`nAnswer under HOOK, PACING, PACKAGING, CTA - at most 2 short points each."
$lines += 'prompt_chars=' + $prompt.Length
$lines | Set-Content $out -Encoding utf8

$imgPath = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'
$b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($imgPath))
$msg = @{ role = 'user'; content = $prompt; images = @($b64, $b64, $b64, $b64) }
$payload = @{
    model      = 'qwen3-vl:4b'
    stream     = $false
    keep_alive = 60
    options    = @{ temperature = 0.2; num_predict = 1500; num_ctx = 8192 }
    messages   = @(@{ role = 'system'; content = $advSys }; $msg)
} | ConvertTo-Json -Depth 8
$sw = [Diagnostics.Stopwatch]::StartNew()
try {
    $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post -ContentType 'application/json' -Body $payload -TimeoutSec 600
    $sw.Stop()
    $think = $resp.message.thinking
    if (-not $think) { $think = $resp.message.reasoning }
    $thinkLen = if ($think) { $think.Length } else { 0 }
    $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
    $lines += ("A qwen3-vl +4img: OK in {0:N1}s done={1} content_len={2} think_len={3}" -f $sw.Elapsed.TotalSeconds, $resp.done_reason, $contentLen, $thinkLen)
    if ($contentLen -eq 0) { $lines += '  CONTENT EMPTY!' }
    else {
        $lines += '---FULL---'
        $lines += $resp.message.content
        $lines += '---END---'
    }
} catch {
    $sw.Stop()
    $body = ''
    if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $body = $_.ErrorDetails.Message }
    $lines += ("A qwen3-vl +4img: ERROR after {0:N1}s - {1} | BODY={2}" -f $sw.Elapsed.TotalSeconds, $_.Exception.Message, $body)
}
$lines | Set-Content $out -Encoding utf8
