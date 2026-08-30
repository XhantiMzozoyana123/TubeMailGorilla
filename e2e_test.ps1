$root = 'C:\Users\Xhanti\source\repos\TubeMailGorillaOfficial'
$apiDir = Join-Path $root 'TubeMailGorilla.Api'
$binDir = Join-Path $apiDir 'bin\Debug\net8.0'
$dll   = Join-Path $binDir 'TubeMailGorilla.Api.dll'
$logOut = Join-Path $root 'e2e.stdout.log'
$logErr = Join-Path $root 'e2e.stderr.log'
$base = 'http://127.0.0.1:5076'

# ---------- 1) Kill leftovers (by name + by port-5076 owner) ----------
Get-CimInstance Win32_Process -Filter "Name='TubeMailGorilla.Api.exe'" -ErrorAction SilentlyContinue | ForEach-Object { & taskkill /PID $_.ProcessId /T /F | Out-Null }
Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -match 'TubeMailGorilla\.Api' } | ForEach-Object { & taskkill /PID $_.ProcessId /T /F | Out-Null }
$po = (netstat -ano | Select-String ':5076\s' | Select-String 'LISTENING' | ForEach-Object { ($_ -split '\s+')[-1] } | Select-Object -First 1)
if ($po) { & taskkill /PID $po /T /F | Out-Null }
Start-Sleep -Seconds 2
Remove-Item $logOut,$logErr -ErrorAction SilentlyContinue

# ---------- 2) Start API from built DLL on 5076 ----------
$env:ASPNETCORE_ENVIRONMENT = 'Development'
Write-Output ('Starting API: dotnet ' + $dll + ' --urls ' + $base)
$proc = Start-Process -FilePath 'dotnet' -ArgumentList @($dll, '--urls', $base, '--environment', 'Development') -WorkingDirectory $binDir -RedirectStandardOutput $logOut -RedirectStandardError $logErr -PassThru -WindowStyle Hidden

# ---------- 3) HTTP readiness ----------
$started = $false
for ($i = 0; $i -lt 50; $i++) {
    if (-not $proc.IsProcessRunning) { Write-Output 'API process EXITED during startup'; break }
    try {
        $probe = Invoke-WebRequest -Uri ($base + '/swagger/index.html') -Method GET -TimeoutSec 3 -ErrorAction Stop
        if ($probe.StatusCode -ge 200 -and $probe.StatusCode -lt 400) { $started = $true; break }
    } catch { }
    Start-Sleep -Seconds 1
}
Write-Output ('HTTP readiness on :5076 = ' + $started)
if (-not $started) {
    Write-Output '--- STDOUT (first 60) ---'
    if (Test-Path $logOut) { Get-Content $logOut | Select-Object -First 60 }
    Write-Output '--- STDERR (first 60) ---'
    if (Test-Path $logErr) { Get-Content $logErr | Select-Object -First 60 }
}

function JsonOk([string]$Desc, [int]$Code, [string]$Body) {
    Write-Output ("`nOK  " + $Desc + " -> HTTP " + $Code + " | " + $Body)
}
function JsonErr([string]$Desc, $Exc) {
    $r = $Exc.Response
    $code = if ($r) { $r.StatusCode.value__ } else { 'ERR' }
    $msg = if ($r) { try { $r.Content.ReadToEndAsync().GetAwaiter().GetResult() } catch { '' } } else { $Exc.Message }
    Write-Output ("`nERR " + $Desc + " -> HTTP " + $code + " | " + $msg)
}

# ---------- 4) Request suite ----------
$stamp = [Int64]([DateTimeOffset]::Now.ToUnixTimeSeconds())
$email = 'e2e-' + $stamp + '@example.com'

try { $s = Invoke-WebRequest -Uri ($base + '/swagger/index.html') -Method GET -TimeoutSec 15 -ErrorAction Stop; JsonOk 'Swagger UI' $s.StatusCode $s.Content } catch { JsonErr 'Swagger UI' $_ }

$regBody = '{ "email":"' + $email + '","password":"Abcdef1","fullName":"E2E Test" }'
try { $r = Invoke-WebRequest -Uri ($base + '/api/auth/register') -Method Post -Body $regBody -Headers @{'Content-Type'='application/json'} -TimeoutSec 20 -ErrorAction Stop; JsonOk 'Register new account (expect 200)' $r.StatusCode $r.Content } catch { JsonErr 'Register new account' $_ }

try { $r2 = Invoke-WebRequest -Uri ($base + '/api/auth/register') -Method Post -Body $regBody -Headers @{'Content-Type'='application/json'} -TimeoutSec 20 -ErrorAction Stop; JsonOk 'Re-register (dup, expect 422)' $r2.StatusCode $r2.Content } catch { JsonErr 'Re-register (dup)' $_ }

$loginBody = '{ "email":"' + $email + '","password":"Abcdef1" }'
try {
    $login = Invoke-WebRequest -Uri ($base + '/api/auth/login') -Method Post -Body $loginBody -Headers @{'Content-Type'='application/json'} -TimeoutSec 20 -ErrorAction Stop
    JsonOk 'Login (correct pwd, expect 200)' $login.StatusCode ('(body len=' + $login.Content.Length + ')')
    $doc = $null; $tok = $null
    try { $doc = System.Text.Json.JsonDocument.Parse($login.Content).RootElement } catch {}
    if ($doc -ne $null) {
        if ($doc.TryGetProperty('token', [ref]$null)) { $tok = $doc.GetProperty('token').GetString() }
        if (-not $tok -and $doc.TryGetProperty('Token', [ref]$null)) { $tok = $doc.GetProperty('Token').GetString() }
        if (-not $tok -and $doc.TryGetProperty('Data', [ref]$null)) { $d = $doc.GetProperty('Data'); if ($d.TryGetProperty('Token', [ref]$null)) { $tok = $d.GetProperty('Token').GetString() } }
    }
    Write-Output ('JWT captured = ' + ($null -ne $tok))
    if ($tok) {
        $auth = @{'Authorization' = 'Bearer ' + $tok; 'Content-Type'='application/json'}
        try { $u = Invoke-WebRequest -Uri ($base + '/api/auth/user') -Headers $auth -TimeoutSec 15 -ErrorAction Stop; JsonOk 'GET /api/auth/user' $u.StatusCode $u.Content } catch { JsonErr 'GET /api/auth/user' $_ }
        try { $st = Invoke-WebRequest -Uri ($base + '/api/payments/status') -Headers $auth -TimeoutSec 15 -ErrorAction Stop; JsonOk 'GET /api/payments/status' $st.StatusCode $st.Content } catch { JsonErr 'GET /api/payments/status' $_ }
        try { $pr = Invoke-WebRequest -Uri ($base + '/api/payments/premium') -Headers $auth -TimeoutSec 15 -ErrorAction Stop; JsonOk 'GET /api/payments/premium (expect 403)' $pr.StatusCode $pr.Content } catch { JsonErr 'GET /api/payments/premium (403 expected)' $_ }
    }
    $badBody = '{ "email":"' + $email + '","password":"Wrong123" }'
    try { $b = Invoke-WebRequest -Uri ($base + '/api/auth/login') -Method Post -Body $badBody -Headers @{'Content-Type'='application/json'} -TimeoutSec 20 -ErrorAction Stop; JsonOk 'Login wrong pwd (expect 422)' $b.StatusCode $b.Content } catch { JsonErr 'Login wrong pwd' $_ }
} catch { JsonErr 'Login (correct pwd)' $_ }

# ---------- 5) Cleanup ----------
if ($proc.IsProcessRunning) { & taskkill /PID $proc.Id /T /F | Out-Null }
$po = (netstat -ano | Select-String ':5076\s' | Select-String 'LISTENING' | ForEach-Object { ($_ -split '\s+')[-1] } | Select-Object -First 1)
if ($po) { & taskkill /PID $po /T /F | Out-Null }
Write-Output "`n===== STDOUT (tail) ====="
if (Test-Path $logOut) { Get-Content $logOut | Select-Object -Last 15 }
Write-Output '===== STDERR (tail) ====='
if (Test-Path $logErr) { Get-Content $logErr | Select-Object -Last 15 }
