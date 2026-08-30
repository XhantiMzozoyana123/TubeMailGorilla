$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5076'
$email = "verify-$(Get-Random)@test.com"
$fail = 0

function Check([string]$desc, [bool]$ok) {
    if ($ok) { Write-Host "PASS  $desc" } else { Write-Host "FAIL  $desc" ; $script:fail++ }
}

# 1. Register fresh user
try {
    $r = Invoke-RestMethod -Uri "$base/api/auth/register" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
    Check "Register new user" ($r.success -eq $true)
} catch { Check "Register new user" $false }

# 2. Login
try {
    $login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
    Check "Login returns JWT" (-not [string]::IsNullOrEmpty($login.token))
} catch { Check "Login returns JWT" $false }

# 3. GetCurrentUser with token
try {
    $h = @{ Authorization = "Bearer $($login.token)" }
    $u = Invoke-RestMethod -Uri "$base/api/auth/user" -Headers $h
    Check "GET /api/auth/user (auth)" ($u.email -eq $email)
} catch { Check "GET /api/auth/user (auth)" $false }

# 4. Pricing endpoint (anonymous)
try {
    $pr = Invoke-RestMethod -Uri "$base/api/payments/pricing"
    Check "GET /api/payments/pricing anonymous" ($pr.amount -gt 0 -and $pr.currency -eq 'USD')
} catch { Check "GET /api/payments/pricing anonymous" $false }

# 5. Status not subscribed
try {
    $st = Invoke-RestMethod -Uri "$base/api/payments/status" -Headers $h
    Check "GET /api/payments/status -> not subscribed" ($st.isSubscribed -eq $false)
} catch { Check "GET /api/payments/status -> not subscribed" $false }

# 6. Premium endpoint (should be 403 without claim)
try {
    Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h | Out-Null
    Check "GET /api/payments/premium blocked (403)" $false
} catch { Check "GET /api/payments/premium blocked (403)" ($_.Exception.Response.StatusCode.value__ -eq 403) }

# 7. Unauthorized when no token
try {
    Invoke-RestMethod -Uri "$base/api/payments/create" -Method Post -Body '{}' -ContentType 'application/json' | Out-Null
    Check "create without token -> 401" $false
} catch { Check "create without token -> 401" ($_.Exception.Response.StatusCode.value__ -eq 401) }

Write-Host "`n=== RESULT: $fail failure(s) ==="
Write-Host "EMAIL=$email" | Out-File "$env:TEMP\verify_email.txt" -Encoding ascii
Write-Host "TOKEN=$($login.token)" | Out-File "$env:TEMP\verify_token.txt" -Encoding ascii
exit $fail