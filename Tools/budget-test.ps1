$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\budget-test.txt'
$lines = @('start=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

$img = [Convert]::ToBase64String([IO.File]::ReadAllBytes('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'))

# O: full video-review shape - the app's real 1674-char prompt + 4 frames,
#    num_predict raised 260 -> 1200 (thinking + answer both fit).
$transcript = ('Tonight we are touring this five bedroom house in Orlando with a full walkthrough of the kitchen and the primary suite and the backyard. ' * 6)
$reviewPrompt = @"
Review this YouTube video and give the editor specific changes that would raise watch time, retention and engagement.

Video details:
- Title: Our Biggest Home Tour Yet!
- Channel: Mark Callender Homes
- Length: 12m 40s
- Description: Walk through of a \$1.2M custom home in Orlando. Full tour with commentary on the build quality, layout decisions and staging.

Transcript:
$transcript

Sample frames from across the video are attached. Comment on what you can actually see in them.

Answer under these four headings, with at most 2 short points under each. Be brief - one sentence per point.

HOOK - the opening line or first shot to change, and why viewers leave now.
PACING - the one cut or reorder that would most improve retention.
PACKAGING - the specific title and thumbnail change.
CTA - the end screen or call to action to add.
"@
$reviewPrompt = $reviewPrompt.Replace('\"', '"')
$sysAdv = 'You are a senior YouTube editor and retention strategist. You give specific, concrete, actionable editing advice about a video. Rules: Ground every point in the material you are given. Be specific and practical. Write plain text. No markdown headers, no bold/asterisks, no emoji. Keep it tight and skimmable.'

function Invoke-Case($name, $sysText, $userText, $numPredict, $withImages) {
    $imgsJson = ''
    if ($withImages) { $imgsJson = ',"images":["' + $img + '","' + $img + '","' + $img + '","' + $img + '"]' }
    $json = '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":' + $numPredict + ',"num_ctx":8192},"messages":[{"role":"system","content":"' + $sysText.Replace('"', '\"') + '"},{"role":"user","content":"' + $userText.Replace('"', '\"').Replace("`r`n", '\n').Replace("`n", '\n') + '"' + $imgsJson + '}]}'
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
        $script:lines += ('{0}: done={1} content_len={2} think_len={3}' -f $name, $resp.done_reason, $contentLen, $thinkLen)
        if ($contentLen -eq 0) { $script:lines += '  CONTENT EMPTY!' }
        else { $script:lines += '  content_head=' + $resp.message.content.Substring(0, [Math]::Min(160, $contentLen)) }
        if ($resp.error) { $script:lines += '  ERR=' + $resp.error }
    } catch {
        $script:lines += ('{0}: PARSE FAIL len={1} head={2}' -f $name, $text.Length, $text.Substring(0, [Math]::Min(200, $text.Length)))
    }
    $script:lines | Set-Content $out -Encoding utf8
}

Invoke-Case 'O review np=1200' $sysAdv $reviewPrompt 1200 $true

# P: extraction-shaped call (like GenerateTextAsync with maxTokens 160 today)
$sysExt = 'You extract contact fields from text. Reply with JSON only.'
$usrExt = 'Extract name and email from: "Contact John Smith at john.smith@example.com for the listing."'
Invoke-Case 'P extraction np=160' $sysExt $usrExt 160 $false
Invoke-Case 'Q extraction np=600' $sysExt $usrExt 600 $false

$lines | Set-Content $out -Encoding utf8
