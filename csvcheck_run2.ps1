$ErrorActionPreference = 'Continue'
$stamp = Get-Date -Format 'HH:mm:ss'
"[$stamp] CSVCHECK START" | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvcheck_run2.txt'
try {
  Set-Location 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\CsvCheck'
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList 'run' -NoNewWindow -PassThru -RedirectStandardOutput 'c:\Users\Shadow\Documents\TubeMailGorilla\csvcheck_result.txt' -RedirectStandardError 'c:\Users\Shadow\Documents\TubeMailGorilla\csvcheck_err.txt'
  "HARNESS PID: $($proc.Id)" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvcheck_run2.txt'
} catch {
  "LAUNCH FAILED: $_" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\csvcheck_run2.txt'
}
'csvcheck relaunched'
