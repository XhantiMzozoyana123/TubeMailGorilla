$bin = 'c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Maui.Unlocked\bin\Debug\net10.0-windows10.0.19041.0\win-x64'
$o = @()
$o += ('exists=' + (Test-Path $bin))
Get-ChildItem $bin -Filter '*.dll' -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -match 'WindowsAppRuntime|WindowsAppSDK|Interop|Untracked' } |
  ForEach-Object { $o += $_.Name }
$o += '--- top of dir ---'
Get-ChildItem $bin -ErrorAction SilentlyContinue | Select-Object -First 15 |
  ForEach-Object { $o += $_.Name }
$o | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\appbin_32.txt' -Encoding utf8
Write-Output 'DONE'
