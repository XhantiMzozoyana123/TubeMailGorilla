$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\ollama-smoke2.txt'
$lines = @()
$lines += 'time=' + (Get-Date).ToString('HH:mm:ss')
try {
    $raw = Invoke-WebRequest -Uri 'http://127.0.0.1:11434/api/tags' -UseBasicParsing -TimeoutSec 10
    $tags = $raw.Content | ConvertFrom-Json
    $lines += 'tags=' + (($tags.models | ForEach-Object { $_.name }) -join ',')
} catch { $lines += 'tags_ERR=' + $_.Exception.Message }
try {
    $body = @{
        model    = 'llama3:latest'
        stream   = $false
        messages = @(
            @{ role = 'system'; content = 'You are a strict data-extraction engine.' },
            @{ role = 'user'; content = 'Extract the name: Text: "Hi, I am Sarah Mitchell." ->' }
        )
        options   = @{ num_predict = 20; temperature = 0.2 }
        keep_alive = 60
    } | ConvertTo-Json -Depth 6
    $resp = Invoke-RestMethod -Uri 'http://127.0.0.1:11434/api/chat' -Method Post `
        -ContentType 'application/json' -Body $body -TimeoutSec 180
    $lines += 'chat=OK reply=[' + $resp.message.content.Trim() + ']'
} catch {
    $lines += 'chat_FAIL=' + $_.Exception.Message
    if ($_.Exception.Response) {
        try {
            $sr = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $lines += 'chat_body=' + $sr.ReadToEnd()
        } catch {}
    }
}
$lines | Set-Content $out
