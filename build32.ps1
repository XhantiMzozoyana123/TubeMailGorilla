$ErrorActionPreference = 'Continue'
$stamp = Get-Date -Format 'HH:mm:ss'
"[$stamp] BUILD32 START (csv BOM fix)" | Out-File -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32.txt'
try {
  Set-Location 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked'
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList 'build -c Debug -f net10.0-windows10.0.19041.0' -NoNewWindow -PassThru -RedirectStandardOutput 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_out.txt' -RedirectStandardError 'c:\Users\Shadow\Documents\TubeMailGorilla\build32_err.txt'
  "BUILD PID: $($proc.Id)" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32.txt'
} catch {
  "LAUNCH FAILED: $_" | Out-File -Append -Encoding utf8 'c:\Users\Shadow\Documents\TubeMailGorilla\build32.txt'
}
'build32 launched'
