$lines = @()
$lines += 'time=' + (Get-Date).ToString('HH:mm:ss')
$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\review-repro.txt'
$lines | Set-Content $out -Encoding utf8   # truncate so we never read stale results

$advSys = 'You are a senior YouTube editor and retention strategist. ' +
    'You give specific, concrete, actionable editing advice about a video. ' +
    'Rules: Ground every point in the material you are given. ' +
    'Be specific and practical. Write plain text. No markdown headers, no bold/asterisks, no emoji. Keep it tight and skimmable.'

# Realistic app-scale prompt (app caps total input at MaxInputCharacters=8000).
$transcript = ('Tonight we are touring this five bedroom house in Orlando with a full walkthrough of the kitchen and the primary suite and the backyard. ' * 6)
$prompt = @"
Review this YouTube video and give the editor specific changes that would raise watch time, retention and engagement.

Video details:
- Title: Our Biggest Home Tour Yet!
- Channel: Mark Callender Homes
- Length: 12m 40s
- Description: Walk through of a $1.2M custom home in Orlando. Full tour with commentary on the build quality, layout decisions and staging.

Transcript:
$transcript

Sample frames from across the video are attached. Comment on what you can actually see in them.

Answer under these four headings, with at most 2 short points under each. Be brief - one sentence per point.

HOOK - the opening line or first shot to change, and why viewers leave now.
PACING - the one cut or reorder that would most improve retention.
PACKAGING - the specific title and thumbnail change.
CTA - the end screen or call to action to add, and where.
"@
$lines += 'prompt_chars=' + $prompt.Length

$imgPath = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'
$b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($imgPath))

function Invoke-OllamaTest($name, $model, $withImages, $bodyOptions) {
    $msg = @{ role = 'user'; content = $prompt }
    if ($withImages) { $msg.images = @($b64, $b64, $b64, $b64) }
    $payload = @{
        model      = $model
        stream     = $false
        keep_alive = 60
        options    = $bodyOptions
        messages   = @(
            @{ role = 'system'; content = $advSys }
            $msg
        )
    } | ConvertTo-Json -Depth 8
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
            -ContentType 'application/json' -Body $payload -TimeoutSec 300
        $sw.Stop()
        $think = $resp.message.thinking
        if (-not $think) { $think = $resp.message.reasoning }
        $thinkLen = if ($think) { $think.Length } else { 0 }
        $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
        $script:lines += ("{0}: OK in {1:N1}s done={2} content_len={3} think_len={4}" -f $name, $sw.Elapsed.TotalSeconds, $resp.done_reason, $contentLen, $thinkLen)
        if ($contentLen -eq 0) { $script:lines += '  CONTENT EMPTY!' }
        else { $script:lines += '  first100=' + $resp.message.content.Substring(0, [Math]::Min(100, $contentLen)) }
    } catch {
        $sw.Stop()
        $body = ''
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $body = $_.ErrorDetails.Message }
        $script:lines += ("{0}: ERROR after {1:N1}s - {2} | BODY={3}" -f $name, $sw.Elapsed.TotalSeconds, $_.Exception.Message, $body)
    }
    $script:lines | Set-Content $out -Encoding utf8   # flush after every test
}

Invoke-OllamaTest 'A qwen3-vl +4img' 'qwen3-vl:4b' $true @{ temperature = 0.2; num_predict = 260 }
Invoke-OllamaTest 'B qwen3-vl noimg' 'qwen3-vl:4b' $false @{ temperature = 0.2; num_predict = 260 }
Invoke-OllamaTest 'C llama3 +4img' 'llama3:latest' $true @{ temperature = 0.2; num_predict = 260 }
Invoke-OllamaTest 'D qwen3-vl +4img num_ctx=8192' 'qwen3-vl:4b' $true @{ temperature = 0.2; num_predict = 260; num_ctx = 8192 }

# E/F: same as D but with the top-level "think": false field (kills qwen3
# reasoning so the 260-token budget goes to the actual answer).
function Invoke-OllamaThinkTest($name, $model, $withImages) {
    $msg = @{ role = 'user'; content = $prompt; images = @($b64, $b64, $b64, $b64) }
    if (-not $withImages) { $msg.Remove('images') }
    $payload = @{
        model      = $model
        stream     = $false
        keep_alive = 60
        think      = $false
        options    = @{ temperature = 0.2; num_predict = 260; num_ctx = 8192 }
        messages   = @(
            @{ role = 'system'; content = $advSys }
            $msg
        )
    } | ConvertTo-Json -Depth 8
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
            -ContentType 'application/json' -Body $payload -TimeoutSec 300
        $sw.Stop()
        $think = $resp.message.thinking
        if (-not $think) { $think = $resp.message.reasoning }
        $thinkLen = if ($think) { $think.Length } else { 0 }
        $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
        $script:lines += ("{0}: OK in {1:N1}s done={2} content_len={3} think_len={4}" -f $name, $sw.Elapsed.TotalSeconds, $resp.done_reason, $contentLen, $thinkLen)
        if ($contentLen -eq 0) { $script:lines += '  CONTENT EMPTY!' }
        else { $script:lines += '  first100=' + $resp.message.content.Substring(0, [Math]::Min(100, $contentLen)) }
    } catch {
        $sw.Stop()
        $body = ''
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $body = $_.ErrorDetails.Message }
        $script:lines += ("{0}: ERROR after {1:N1}s - {2} | BODY={3}" -f $name, $sw.Elapsed.TotalSeconds, $_.Exception.Message, $body)
    }
    $script:lines | Set-Content $out -Encoding utf8
}
Invoke-OllamaThinkTest 'E qwen3-vl think=false +ctx' 'qwen3-vl:4b' $true
Invoke-OllamaThinkTest 'F llama3 think=false noimg' 'llama3:latest' $false

$lines | Set-Content $out -Encoding utf8
