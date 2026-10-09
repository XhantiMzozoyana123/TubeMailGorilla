$ErrorActionPreference = 'Continue'
$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
Set-Location $root
$out = @()
$out += '=== add ==='
$out += (git add -A 2>&1 | Out-String)
$out += '=== staged stat ==='
$out += (git diff --cached --stat 2>&1 | Out-String)
$out += '=== commit ==='
$out += (git commit -m "MAUI Unlocked: fix custom icebreaker empty-response failure, add template token chips + direct-send template picker" -m "Icebreaker reliability: text workload moves to llama3:latest (qwen3-vl thinking trace cannot be disabled on Ollama 0.40 and burned the token budget on custom prompts). Icebreakers get their own copywriter system prompt, a retry with +50% budget on empty responses, LLMService.LastError surfaced in the failure dialog instead of the generic timeout message." -m "EmailTemplateDetailsPage: subject + body token chip rows (contact/send-page parity, custom parameters merged, cursor-position insert) plus hint about per-recipient resolution." -m "ContactDetailsPage direct-send form: START FROM A TEMPLATE picker that fills subject/message, mirroring SendEmailsPage." -m "Also: ComfyUI/VideoDownload services, EmailService snapshot + cid-embedding updates, Tools/LLMCheck harness." 2>&1 | Out-String)
$out += '=== push ==='
$out += (git push origin master 2>&1 | Out-String)
$out | Out-File -Encoding utf8 "$root\push_result.txt"
'wrote push_result.txt'
