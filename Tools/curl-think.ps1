$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\curl-think.txt'
$lines = @('time=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

function Invoke-CurlCase($name, $json) {
    $tmp = [IO.Path]::GetTempFileName()
    [IO.File]::WriteAllText($tmp, $json)
    $raw = & curl.exe -s -X POST 'http://localhost:11434/api/chat' -H 'Content-Type: application/json' -d "@$tmp"
    Remove-Item $tmp -Force
    try {
        $resp = $raw | ConvertFrom-Json
        $think = $resp.message.thinking
        if (-not $think) { $think = $resp.message.reasoning }
        $thinkLen = if ($think) { $think.Length } else { 0 }
        $contentLen = if ($resp.message.content) { $resp.message.content.Length } else { 0 }
        $script:lines += ('{0}: done={1} content_len={2} think_len={3}' -f $name, $resp.done_reason, $contentLen, $thinkLen)
        if ($contentLen -eq 0) { $script:lines += '  CONTENT EMPTY!' }
        else { $script:lines += '  content=' + $resp.message.content }
        if ($resp.error) { $script:lines += '  ERR=' + $resp.error }
    } catch {
        $script:lines += ('{0}: PARSE FAIL raw={1}' -f $name, ($raw -join ' ').Substring(0, [Math]::Min(300, ($raw -join '').Length)))
    }
    $script:lines | Set-Content $out -Encoding utf8
}

# 1: raw think=false (no PowerShell JSON involved)
Invoke-CurlCase 'K think=false raw' '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"messages":[{"role":"user","content":"Reply with exactly: hello world"}]}'
# 2: no think field (model default)
Invoke-CurlCase 'L default (no think)' '{"model":"qwen3-vl:4b","stream":false,"keep_alive":60,"messages":[{"role":"user","content":"Reply with exactly: hello world"}]}'
# 3: /no_think in the USER message
Invoke-CurlCase 'M /no_think user msg' '{"model":"qwen3-vl:4b","stream":false,"keep_alive":60,"messages":[{"role":"user","content":"/no_think Reply with exactly: hello world"}]}'
# 4: think=false WITH the big picture+prompt (the app-shaped request, num_ctx=8192)
$img = [Convert]::ToBase64String([IO.File]::ReadAllBytes('c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'))
$bigPrompt = 'Review this YouTube video and give the editor specific changes that would raise watch time. HOOK - the opening line to change. PACING - the one cut. PACKAGING - the title and thumbnail change. CTA - the end screen to add.'
$escapedImg = '"images":["' + $img + '","' + $img + '","' + $img + '","' + $img + '"]'
Invoke-CurlCase 'N app-shape think=false ctx8192' ('{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":260,"num_ctx":8192},"messages":[{"role":"system","content":"You are a senior YouTube editor."},{"role":"user","content":"' + $bigPrompt + '",' + $escapedImg + '}]}')

$lines | Set-Content $out -Encoding utf8
