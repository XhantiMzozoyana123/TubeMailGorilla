$o = @()
$procs = Get-Process python* -ErrorAction SilentlyContinue
$o += ('python_count=' + (@($procs).Count))
foreach ($pr in $procs) { $o += ('  pid=' + $pr.Id + ' cpu=' + [math]::Round($pr.CPU,1)) }
$o += ('cuda_out_10 size=' + (Get-Item 'C:\Users\Shadow\Documents\TubeMailGorilla\cuda_out_10.txt').Length)
$o += ('cuda_out_10 content:')
$o += (Get-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\cuda_out_10.txt')
$o | Set-Content 'C:\Users\Shadow\Documents\TubeMailGorilla\proc_status_11.txt'
Write-Output 'PROC DONE'
