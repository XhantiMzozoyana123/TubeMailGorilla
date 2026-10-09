$lines = @()
$lines += 'time=' + (Get-Date).ToString('HH:mm:ss')
$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\think-test.txt'
$lines | Set-Content $out -Encoding utf8

try {
    $ver = Invoke-RestMethod -Uri 'http://localhost:11434/api/version' -TimeoutSec 5
    $lines += 'ollama version=' + $ver.version
} catch { $lines += 'version FAIL: ' + $_.Exception.Message }

$imgPath = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'
$b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($imgPath))
$short = 'Answer in one short sentence: what should an editor change first in a 12 minute home tour video?'

function Invoke-ThinkCase($name, $extraFields, $systemText, $withImages) {
    $payload = @{
        model      = 'qwen3-vl:4b'
        stream     = $false
        keep_alive = 60
        options    = @{ temperature = 0.2; num_predict = 260; num_ctx = 8192 }
        messages   = @(
            @{ role = 'system'; content = $systemText }
            @{ role = 'user'; content = $short }
        )
    }
    if ($withImages) { $payload.messages[1].images = @($b64) }
    foreach ($k in $extraFields.Keys) { $payload[$k] = $extraFields[$k] }
    $json = $payload | ConvertTo-Json -Depth 8
    # Echo what we actually send for the think key
    $m = [regex]::Match($json, '"think"\s*:\s*(true|false|null)')
    $lines += ('{0}: payload think-field=[{1}]' -f $name, $(if ($m.Success) { $m.Groups[1].Value } else { 'ABSENT' }))
    try {
        $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
            -ContentType 'application/json' -Body $json -TimeoutSec 300
        $think = $resp.message.thinking
        if (-not $think) { $think = $resp.message.reasoning }
        $thinkLen = if ($think) { $think.Length } else { 0 }
        $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
        $script:lines += ('{0}: OK done={1} content_len={2} think_len={3}' -f $name, $resp.done_reason, $contentLen, $thinkLen)
        if ($contentLen -eq 0) { $script:lines += '  CONTENT EMPTY!' }
        else { $script:lines += '  content=' + $resp.message.content.Substring(0, [Math]::Min(140, $contentLen)) }
    } catch {
        $body = ''
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $body = $_.ErrorDetails.Message }
        $script:lines += ('{0}: ERROR - {1} | {2}' -f $name, $_.Exception.Message, $body)
    }
    $script:lines | Set-Content $out -Encoding utf8
}

Invoke-ThinkCase 'G think=false plain' @{ think = $false } 'You are a video editing assistant.' $true
Invoke-ThinkCase 'H /no_think in system' @{} ('You are a video editing assistant. /no_think') $true
Invoke-ThinkCase 'I think=false + /no_think' @{ think = $false } ('You are a video editing assistant. /no_think') $true
Invoke-ThinkCase 'J think=true control' @{ think = $true } 'You are a video editing assistant.' $true

$lines | Set-Content $out -Encoding utf8
