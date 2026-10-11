$ErrorActionPreference = 'Continue'
$stamp = Get-Date -Format 'HH:mm:ss'
"[$stamp] BUILD31 START" | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build31.txt'
try {
  Set-Location 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked'
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList 'build -c Debug -f net10.0-windows10.0.19041.0' -NoNewWindow -PassThru -RedirectStandardOutput 'c:\Users\Shadow\Documents\TubeMailGorilla\build31_out.txt' -RedirectStandardError 'c:\Users\Shadow\Documents\TubeMailGorilla\build31_err.txt'
  "BUILD PID: $($proc.Id)" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build31.txt'
} catch {
  "LAUNCH FAILED: $_" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build31.txt'
}
'build31 launched'
