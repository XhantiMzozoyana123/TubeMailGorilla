$lines = @()
$lines += 'time=' + (Get-Date).ToString('HH:mm:ss')
try {
    # Test A: app's exact shape (num_predict=512, temp 0.2).
    $imgPath = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\Resources\Images\dotnet_bot.png'
    $b64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($imgPath))

    $body = @{
        model      = 'qwen3-vl:4b'
        stream     = $false
        keep_alive = 60
        options    = @{ temperature = 0.2; num_predict = 512 }
        messages   = @(
            @{ role = 'system'; content = 'You are a vision assistant. Answer in one short sentence.' }
            @{ role = 'user'; content = 'Describe this image in one short sentence.'; images = @($b64) }
        )
    } | ConvertTo-Json -Depth 6

    $resp = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
        -ContentType 'application/json' -Body $body -TimeoutSec 300
    $reason = $resp.message.reasoning
    $lines += 'A512=OK content=[' + $resp.message.content + '] reasoning_len=' +
        $(if ($reason) { $reason.Length } else { 0 })
} catch {
    $lines += 'A512 ERROR=' + $_.Exception.Message
}

try {
    # Test B: same but think:false (supported by Ollama for thinking models).
    $body2 = @{
        model      = 'qwen3-vl:4b'
        stream     = $false
        keep_alive = 60
        think      = $false
        options    = @{ temperature = 0.2; num_predict = 512 }
        messages   = @(
            @{ role = 'system'; content = 'You are a vision assistant. Answer in one short sentence.' }
            @{ role = 'user'; content = 'Describe this image in one short sentence.'; images = @($b64) }
        )
    } | ConvertTo-Json -Depth 6

    $resp2 = Invoke-RestMethod -Uri 'http://localhost:11434/api/chat' -Method Post `
        -ContentType 'application/json' -Body $body2 -TimeoutSec 300
    $lines += 'BthinkOFF=OK content=[' + $resp2.message.content + ']'
} catch {
    $lines += 'BthinkOFF ERROR=' + $_.Exception.Message
}
$lines | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\vision-smoke.txt' -Encoding utf8
