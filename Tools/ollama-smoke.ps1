$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\ollama-smoke.txt'
$lines = @()
try {
    $tags = Invoke-RestMethod -Uri 'http://localhost:11434/api/tags' -TimeoutSec 10
    $lines += 'server=OK'
    $lines += 'models=' + (($tags.models | ForEach-Object { $_.name }) -join ', ')
} catch {
    $lines += 'server=FAIL ' + $_.Exception.Message
}
try {
    $body = @{
        model   = 'llama3:latest'
        stream  = $false
        messages = @(
            @{ role = 'system'; content = 'You are a strict data-extraction engine.' },
            @{ role = 'user';   content = 'Extract the name: Text: "Hi, I am Sarah Mitchell." ->' }
        )
        options  = @{ num_predict = 20; temperature = 0.2 }
        keep_alive = 60
    } | ConvertTo-Json -Depth 6
    $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
        -ContentType 'application/json' -Body $body -TimeoutSec 120
    $lines += 'chat=OK reply=[' + $resp.message.content.Trim() + ']'
} catch {
    $lines += 'chat=FAIL ' + $_.Exception.Message
}
$lines | Set-Content $out
