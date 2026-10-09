$root = 'c:\Users\Shadow\Documents\TubeMailGorilla'
$log = Join-Path $root 'build_25.log'
$o = @()
if (Test-Path $log) {
  $c = Get-Content $log
  $o += '=== ERROR lines ==='
  $errs = $c | Select-String -Pattern ' error |Error\(s\)' | ForEach-Object { $_.ToString() }
  if ($errs) { $o += $errs } else { $o += '(no error lines)' }
  $o += '=== tail ==='
  $o += ($c | Select-Object -Last 5)
} else { $o += 'log missing' }
$pidv = Get-Content (Join-Path $root 'build_25.pid') -ErrorAction SilentlyContinue
if ($pidv) {
  $proc = Get-Process -Id $pidv -ErrorAction SilentlyContinue
  $o += ('build process alive=' + [bool]$proc)
}
$o | Set-Content (Join-Path $root 'build25_status_30.txt') -Encoding utf8
Write-Output 'STATUS DONE'
