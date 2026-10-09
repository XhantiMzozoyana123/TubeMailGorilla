# Reproduce EXACTLY what LLMService.InferAsync sends: /api/chat, stream=false,
# think=false (app sends it), num_predict=1200 (AIService icebreaker budget),
# temperature=0.2 (LlmSettings), num_ctx=8192, extraction system prompt.
# Two runs: standard icebreaker + custom-instructions variant (the user's path).

function Invoke-OllamaChat($prompt, $label) {
    $req = @{
        model = 'qwen3-vl:4b'
        stream = $false
        think = $false
        keep_alive = 60
        messages = @(
            @{ role = 'system'; content = 'You are a strict data-extraction engine. Return ONLY the raw data value the user asks for.' },
            @{ role = 'user'; content = $prompt }
        )
        options = @{ temperature = 0.2; num_predict = 1200; num_ctx = 8192 }
    } | ConvertTo-Json -Depth 8
    $path = 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_req2.json'
    [System.IO.File]::WriteAllText($path, $req, (New-Object System.Text.UTF8Encoding($false)))

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $resp = & curl.exe -s --max-time 300 -X POST http://localhost:11434/api/chat -H 'Content-Type: application/json' --data-binary ('@' + $path)
    $sw.Stop()
    $obj = $null
    try { $obj = $resp | ConvertFrom-Json } catch { }
    $content = if ($obj -and $obj.message) { $obj.message.content } else { '' }
    $thinking = if ($obj -and $obj.message) { $obj.message.thinking } else { '' }
    $out = @()
    $out += ('[' + $label + '] elapsed_s=' + [Math]::Round($sw.Elapsed.TotalSeconds, 1))
    $out += ('[' + $label + '] done_reason=' + $(if ($obj) { $obj.done_reason } else { 'PARSE-FAIL' }) +
             ' eval_count=' + $(if ($obj) { $obj.eval_count } else { '?' }) +
             ' content_len=' + $(if ($null -ne $content) { $content.Length } else { -1 }) +
             ' thinking_len=' + $(if ($null -ne $thinking) { $thinking.Length } else { -1 }))
    $out += ('[' + $label + '] content=' + $content)
    if ($obj -and -not $obj.message) { $out += ('[' + $label + '] raw=' + $resp) }
    return $out
}

$stdPrompt = @"
You are an expert cold-email copywriter helping a freelance video editor land YouTube creators as retainer clients.

Write ONE personalized first-line icebreaker (the opening sentence of a cold email) for the creator below.

Creator context:
- Name: John
- Channel: Woodworking Masters
- Latest video title: How I built a live-edge table

Rules:
- No greeting (Hi/Hey), no sign-off, no mention of editing services yet
- Plain text only, no quotes, no emojis

Return ONLY the icebreaker text.
"@

$customPrompt = @"
You are an expert cold-email copywriter helping a freelance video editor land YouTube creators as retainer clients.

Write ONE personalized first-line icebreaker (the opening sentence of a cold email) for the creator below.

Creator context:
- Name: John
- Channel: Woodworking Masters
- Latest video title: How I built a live-edge table

Custom instructions from the user (follow them unless they ask for a greeting, sign-off, contact details, or more than 2 sentences):
Open with a pun about sawdust and include the word "sheesh".

Rules:
- No greeting (Hi/Hey), no sign-off, no mention of editing services yet
- Plain text only, no quotes, no emojis

Return ONLY the icebreaker text.
"@

$r = @()
$r += ('ollama version: ' + (& ollama --version 2>&1))
$r += Invoke-OllamaChat $stdPrompt 'STANDARD'
$r += Invoke-OllamaChat $customPrompt 'CUSTOM'
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test2_48.txt' -Encoding utf8
Write-Output 'WORKER2 DONE'
