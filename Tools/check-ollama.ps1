$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\ollama-status.txt'
$lines = @()
$exe = "C:\Users\Shadow\AppData\Local\Programs\Ollama\ollama.exe"
$lines += "ollama.exe exists: $(Test-Path $exe)"
$p = Get-Process ollama -ErrorAction SilentlyContinue
if ($p) { $lines += "process: RUNNING pid=$($p.Id)" } else { $lines += "process: NOT RUNNING" }
try {
  $c = New-Object Net.Sockets.TcpClient
  $c.Connect("127.0.0.1", 11434)
  $lines += "port 11434: OPEN"
  $c.Close()
} catch { $lines += "port 11434: CLOSED" }
$w = Get-Process winget -ErrorAction SilentlyContinue
if ($w) { $lines += "winget: STILL RUNNING" } else { $lines += "winget: not running" }
$ps = Get-Process powershell -ErrorAction SilentlyContinue
$lines += "powershell procs: $($ps.Count)"
try {
  $tail = Get-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\ollama-install.log' -Tail 5
  $lines += "--- log tail ---"
  $lines += $tail
} catch { $lines += "log tail failed" }
$lines | Set-Content $out
