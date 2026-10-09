$r = @()
$r += '=== netsh winhttp proxy ==='
$r += (& netsh winhttp show proxy 2>&1)
$r += '=== IE/user proxy registry ==='
$k = Get-Item 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings'
$r += ('ProxyEnable=' + $k.GetValue('ProxyEnable'))
$r += ('ProxyServer=' + $k.GetValue('ProxyServer'))
$r += ('ProxyOverride=' + $k.GetValue('ProxyOverride'))
$r += ('AutoConfigURL=' + $k.GetValue('AutoConfigURL'))
$r += '=== process env ==='
$r += ('HTTP_PROXY=' + [Environment]::GetEnvironmentVariable('HTTP_PROXY'))
$r += ('HTTPS_PROXY=' + [Environment]::GetEnvironmentVariable('HTTPS_PROXY'))
$r += ('NO_PROXY=' + [Environment]::GetEnvironmentVariable('NO_PROXY'))
$r += ('ALL_PROXY=' + [Environment]::GetEnvironmentVariable('ALL_PROXY'))
# What .NET's HttpClient.DefaultProxy would resolve to (same source the app uses):
try {
    $dp = [System.Net.Http.HttpClient]::DefaultProxy
    if ($dp) {
        $r += ('dotnet DefaultProxy=' + $dp.GetType().FullName)
        $r += ('dotnet GetProxy(http://localhost:11434)=' + $dp.GetProxy([Uri]'http://localhost:11434'))
    } else { $r += 'dotnet DefaultProxy=NULL (direct)' }
} catch { $r += ('dotnet proxy probe failed: ' + $_.Exception.Message) }
$r | Set-Content 'c:\Users\Shadow\Documents\TubeMailGorilla\proxy_51.txt' -Encoding utf8
Write-Output 'DONE'
