$log = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build-ollama.log'
$status = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build-status.txt'
# Wait up to 5 minutes for the log to stop growing and dotnet to exit.
$last = -1
$stable = 0
for ($i = 0; $i -lt 30; $i++) {
    Start-Sleep -Seconds 10
    $len = (Get-Item $log).Length
    $procs = @(Get-Process dotnet -ErrorAction SilentlyContinue).Count
    if ($len -eq $last -and $procs -eq 0) { $stable++ } else { $stable = 0 }
    $last = $len
    if ($stable -ge 2) { break }
}
$lines = @(
    'dotnet_procs=' + @(Get-Process dotnet -ErrorAction SilentlyContinue).Count,
    'log_bytes=' + (Get-Item $log).Length
)
$lines | Set-Content $status
