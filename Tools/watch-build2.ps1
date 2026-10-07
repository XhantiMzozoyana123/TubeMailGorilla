$log = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build-ollama.log'
$status = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build-status.txt'
$iters = 0
for ($i = 0; $i -lt 60; $i++) {
    $iters = $i
    Start-Sleep -Seconds 15
    $txt = ''
    try { $txt = [IO.File]::ReadAllText($log) } catch {}
    $marker = $txt -match 'Build succeeded|Build FAILED|error CS'
    $procs = @(Get-Process dotnet -ErrorAction SilentlyContinue).Count
    if ($marker -or $procs -eq 0) { break }
}
$final = ''
try { $final = [IO.File]::ReadAllText($log) } catch {}
$lines = @(
    'iters=' + $iters,
    'dotnet_procs=' + @(Get-Process dotnet -ErrorAction SilentlyContinue).Count,
    'has_success=' + ($final -match 'Build succeeded'),
    'has_failed=' + ($final -match 'Build FAILED'),
    'has_cserror=' + ($final -match 'error CS')
)
$lines | Set-Content $status
$tail = $final -split "`n"
$start = [Math]::Max(0, $tail.Count - 80)
($tail[$start..($tail.Count - 1)] -join "`n") | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\build-ollama-tail.txt'
