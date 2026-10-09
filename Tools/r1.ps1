$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\r1.txt'
$lines = @('start=' + (Get-Date).ToString('HH:mm:ss'))
$lines | Set-Content $out -Encoding utf8

# Exact SYSTEM_PROMPT from LLMService.cs + a realistic name-extraction prompt
$sys = 'You are a strict data-extraction engine. Return ONLY the raw data value the user asks for. Rules: - NEVER summarize, paraphrase, quote, or list anything from the provided text. - NEVER explain or add context around the answer. - Your ENTIRE response must be the single data value and nothing else - typically 1 to 5 words. - NEVER output lists, bullet points, asterisks, quotation marks, or multiple values. - No introductions. - If the requested data cannot be found as a clear, explicit value in the text, output NOTHING at all - an empty response. Do NOT write BLANK, UNKNOWN, N/A, or NONE. Do NOT substitute related content. Do NOT guess. Empty means empty.'
$usr = 'Return the full name from: Mark Callender Homes tour of Orlando — video description and subtitles mention Mark Callender as the host and listing agent.'
$json = '{"model":"qwen3-vl:4b","stream":false,"think":false,"keep_alive":60,"options":{"temperature":0.2,"num_predict":1500,"num_ctx":8192},"messages":[{"role":"system","content":"' + $sys.Replace('"', '\"') + '"},{"role":"user","content":"' + $usr.Replace('"', '\"') + '"}]}'
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
    $lines += ('done={0} content_len={1} think_len={2}' -f $resp.done_reason, $contentLen, $thinkLen)
    if ($contentLen -eq 0) { $lines += 'CONTENT EMPTY!' } else { $lines += 'content=' + $resp.message.content }
    if ($resp.error) { $lines += 'ERR=' + $resp.error }
} catch {
    $lines += 'PARSE FAIL: ' + $_.Exception.Message
}
$lines | Set-Content $out -Encoding utf8
