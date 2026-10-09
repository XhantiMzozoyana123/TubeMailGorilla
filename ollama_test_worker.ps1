# Direct Ollama test: same call LLMService makes (/api/chat, qwen3-vl:4b).
$body = @{
    model = 'qwen3-vl:4b'
    stream = $false
    messages = @(
        @{ role = 'system'; content = 'You are a strict data-extraction engine.' },
        @{ role = 'user'; content = 'Write ONE personalized first-line icebreaker for creator John who makes woodworking videos.' }
    )
    options = @{ temperature = 0.7; num_predict = 160 }
} | ConvertTo-Json -Depth 6
$body | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_req.json' -Encoding utf8
# Set-Content in Windows PowerShell writes a BOM; Ollama's JSON parser rejects it.
[System.IO.File]::WriteAllText('c:\Users\Shadow\Documents\TubeMailGorilla\ollama_req.json', $body, (New-Object System.Text.UTF8Encoding($false)))

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$resp = & curl.exe -s --max-time 240 -X POST http://localhost:11434/api/chat -H 'Content-Type: application/json' --data-binary '@c:\Users\Shadow\Documents\TubeMailGorilla\ollama_req.json'
$sw.Stop()
$r = @('elapsed_s=' + [Math]::Round($sw.Elapsed.TotalSeconds, 1), ('exit=' + $LASTEXITCODE))
$r += ('resp=' + $resp)
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\ollama_test_45.txt' -Encoding utf8
Write-Output 'WORKER DONE'
