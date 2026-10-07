$out = 'c:\Users\Shadow\Documents\TubeMailGorilla\Tools\dotnet-procs.txt'
$lines = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" |
  Select-Object ProcessId, @{n='CPU';e={[math]::Round((Get-Process -Id $_.ProcessId -ErrorAction SilentlyContinue).CPU,1)}}, CommandLine |
  Format-List | Out-String -Width 400
$lines | Set-Content $out
