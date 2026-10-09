$ErrorActionPreference = 'SilentlyContinue'
$out = @()
$out += 'CURL: ' + ((Get-Command curl.exe).Source)
$out += 'TAR: ' + ((Get-Command tar.exe).Source)
$c = Get-CimInstance Win32_LogicalDisk -Filter 'DeviceID="C:"'
$out += ('FREE_GB: ' + [math]::Round($c.FreeSpace/1GB,1))
$out += ('7Z_PF: ' + (Test-Path 'C:\Program Files\7-Zip\7z.exe'))
$out += ('PY: ' + ((Get-Command python).Source))
$out | Set-Content -Path (Join-Path $PSScriptRoot 'probe3.txt')
Write-Output 'PROBE DONE'
