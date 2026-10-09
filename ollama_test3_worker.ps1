# Test whether Qwen3's /no_think prompt suffix suppresses thinking on
# Ollama 0.40.1 (the think:false request flag is proven ignored).
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
    $path = 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_req3.json'
    [System.IO.File]::WriteAllText($path, $req, (New-Object System.Text.UTF8Encoding($false)))

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $resp = & curl.exe -s --max-time 300 -X POST http://localhost:11434/api/chat -H 'Content-Type: application/json' --data-binary ('@' + $path)
    $sw.Stop()
    $obj = $null
    try { $obj = $resp | ConvertFrom-Json } catch { }
    $content = if ($obj -and $obj.message) { $obj.message.content } else { '' }
    $thinking = if ($obj -and $obj.message) { $obj.message.thinking } else { '' }
    $out = @()
    $out += ('[' + $label + '] elapsed_s=' + [Math]::Round($sw.Elapsed.TotalSeconds, 1) +
             ' done_reason=' + $(if ($obj) { $obj.done_reason } else { 'PARSE-FAIL' }) +
             ' eval_count=' + $(if ($obj) { $obj.eval_count } else { '?' }) +
             ' content_len=' + $(if ($null -ne $content) { $content.Length } else { -1 }) +
             ' thinking_len=' + $(if ($null -ne $thinking) { $thinking.Length } else { -1 }))
    $out += ('[' + $label + '] content=' + $content)
    if ($obj -and -not $obj.message) { $out += ('[' + $label + '] raw=' + $resp) }
    return $out
}

$noThinkPrompt = @"
You are an expert cold-email copywriter helping a freelance video editor land YouTube creators as retainer clients.

Write ONE personalized first-line icebreaker (the opening sentence of a cold email) for the creator below.

Creator context:
- Name: Sarah
- Channel: Cozy Kitchen Recipes
- Latest video title: 15-minute garlic butter pasta

Rules:
- No greeting (Hi/Hey), no sign-off, no mention of editing services yet
- Plain text only, no quotes, no emojis

Return ONLY the icebreaker text.
/no_think
"@

$r = @()
$r += Invoke-OllamaChat $noThinkPrompt 'NO_THINK'
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test3_50.txt' -Encoding utf8
Write-Output 'WORKER3 DONE'
