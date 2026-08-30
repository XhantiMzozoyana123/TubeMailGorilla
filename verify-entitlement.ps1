$ErrorActionPreference = 'Stop'
$base  = 'http://localhost:5076'
$mysql = 'C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe'
$email = "ent-$(Get-Random)@test.com"
$fail  = 0

function Check([string]$desc, [bool]$ok) {
    if ($ok) { Write-Host "PASS  $desc" } else { Write-Host "FAIL  $desc"; $script:fail++ }
}

# 0. Register + login
$null = Invoke-RestMethod -Uri "$base/api/auth/register" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
Check "Fresh user registered + logged in" (-not [string]::IsNullOrEmpty($login.token))

# 1. Grant claim directly in DB (simulate completed subscription)
$uid = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT Id FROM AspNetUsers WHERE Email='$email'")
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ('$uid','subscription','active')"
$cnt = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT COUNT(*) FROM AspNetUserClaims WHERE UserId='$uid' AND ClaimType='subscription' AND ClaimValue='active'")
Check "Claim inserted into AspNetUserClaims" ([int]$cnt -eq 1)

# 2. Re-login -> token carries claim -> status subscribed + premium 200
$login1 = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($login1.token)" }
$st = Invoke-RestMethod -Uri "$base/api/payments/status" -Headers $h
Check "Re-login status shows subscribed" ($st.isSubscribed -eq $true)
try {
    $null = Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h
    Check "Premium endpoint unlocked (200)" $true
} catch { Check "Premium endpoint unlocked (200)" $false }

# 3. Seed ACTIVE row -> cancel tries PayPal first, must fail loudly (no silent downgrade)
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO Subscriptions (UserId, PayPalOrderId, Amount, Currency, Status, CreatedAtUtc, ActivatedAtUtc) VALUES ('$uid','SUBSCRIPTION_TEST','9.99','USD','Active','2026-08-22 00:00:00','2026-08-22 00:00:00')"
$c = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h
Check "Cancel refuses silent downgrade when PayPal fails" ($c.success -eq $false -and $c.isSubscribed -eq $true)
Write-Host "      cancel message: $($c.message)"

# 4. Remove fake active row -> cancel proceeds via claim-removal path
& $mysql -u root TubeMailGorillaDB -e "DELETE FROM Subscriptions WHERE PayPalOrderId='SUBSCRIPTION_TEST'"
$login2 = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h2 = @{ Authorization = "Bearer $($login2.token)" }
$c2 = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h2
Check "Cancel removed claim (not subscribed)" ($c2.success -eq $true -and $c2.isSubscribed -eq $false)

# 5. New token no longer unlocks premium
$h3 = @{ Authorization = "Bearer $($c2.token)" }
try {
    $null = Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h3
    Check "After cancel premium locked (403)" $false
} catch { Check "After cancel premium locked (403)" ($_.Exception.Response.StatusCode.value__ -eq 403) }

Write-Host ""
Write-Host "=== RESULT: $fail failure(s) ==="
exit $fail
$base = 'http://localhost:5076'
$mysql = 'C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe'
$email = "ent-$(Get-Random)@test.com"
$fail = 0
function Check([string]$desc, [bool]$ok) {
    if ($ok) { Write-Host "PASS  $desc" } else { Write-Host "FAIL  $desc" ; $script:fail++ }
}

# 0. Register + login
Invoke-RestMethod -Uri "$base/api/auth/register" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json' | Out-Null
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
Check "Fresh user registered + logged in" (-not [string]::IsNullOrEmpty($login.token))

# 1. Grant claim directly in DB (simulate completed subscription)
$uid = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT Id FROM AspNetUsers WHERE Email='$email'")
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ('$uid','subscription','active')"
$cnt = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT COUNT(*) FROM AspNetUserClaims WHERE UserId='$uid' AND ClaimType='subscription' AND ClaimValue='active'")
Check "Claim inserted into AspNetUserClaims" ([int]$cnt -eq 1)

# 2. Re-login -> token carries claim -> status subscribed + premium 200
$login1 = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($login1.token)" }
$st = Invoke-RestMethod -Uri "$base/api/payments/status" -Headers $h
Check "Re-login status shows subscribed" ($st.isSubscribed -eq $true)
try {
    $prem = Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h
    Check "Premium endpoint unlocked (200)" ($null -ne $prem.message)
} catch { Check "Premium endpoint unlocked (200)" $false }

# 3. Seed an ACTIVE subscription row -> cancel tries PayPal first, fails loudly (no silent downgrade)
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO Subscriptions (UserId, PayPalOrderId, Amount, Currency, Status, CreatedAtUtc, ActivatedAtUtc) VALUES ('$uid','SUBSCRIPTION_TEST','9.99','USD','Active','2026-08-22 00:00:00','2026-08-22 00:00:00')"
$c = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h
Check "Cancel refuses to silently downgrade when PayPal fails" ($c.success -eq $false -and $c.isSubscribed -eq $true)
Write-Host "      cancel message: $($c.message)"

# 4. Remove the fake active row -> cancel proceeds via claim-removal path
& $mysql -u root TubeMailGorillaDB -e "DELETE FROM Subscriptions WHERE PayPalOrderId='SUBSCRIPTION_TEST'"
$login2 = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h2 = @{ Authorization = "Bearer $($login2.token)" }
$c2 = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h2
Check "Cancel removed claim (success, no longer subscribed)" ($c2.success -eq $true -and $c2.isSubscribed -eq $false)

# 5. New token no longer unlocks premium
$h3 = @{ Authorization = "Bearer $($c2.token)" }
try {
    Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h3 | Out-Null
    Check "After cancel premium locked (403)" $false
} catch { Check "After cancel premium locked (403)" ($_.Exception.Response.StatusCode.value__ -eq 403) }

Write-Host "`n=== RESULT: $fail failure(s) ==="
exit $fail
$base = 'http://localhost:5076'
$mysql = 'C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe'
$email = (Get-Content "$env:TEMP\verify_email.txt").Split('=')[1]
$oldToken = (Get-Content "$env:TEMP\verify_token.txt").Split('=')[1].Trim()
$fail = 0
function Check([string]$desc, [bool]$ok) {
    if ($ok) { Write-Host "PASS  $desc" } else { Write-Host "FAIL  $desc" ; $script:fail++ }
}

# 1. Find user id, grant subscription claim directly (simulates a completed subscription)
$uid = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT Id FROM AspNetUsers WHERE Email='$email'")
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO AspNetUserClaims (UserId, ClaimType, ClaimValue) VALUES ('$uid','subscription','active')"
$cnt = (& $mysql -u root -N -B TubeMailGorillaDB -e "SELECT COUNT(*) FROM AspNetUserClaims WHERE UserId='$uid' AND ClaimType='subscription' AND ClaimValue='active'")
Check "Claim inserted into AspNetUserClaims" ([int]$cnt -eq 1)

# 2. Insert an Active subscription row so cancel finds one + exercises the PayPal-cancel guard
& $mysql -u root TubeMailGorillaDB -e "INSERT INTO Subscriptions (UserId, PayPalOrderId, Amount, Currency, Status, CreatedAtUtc, ActivatedAtUtc) VALUES ('$uid','SUBSCRIPTION_TEST','9.99','USD','Active','2026-08-22 00:00:00','2026-08-22 00:00:00')" 2>$null
Check "Subscription row seeded" ($LASTEXITCODE -eq 0)

# 3. Re-login -> token should carry claim -> premium 200
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($login.token)" }
$st = Invoke-RestMethod -Uri "$base/api/payments/status" -Headers $h
Check "Re-login status shows subscribed" ($st.isSubscribed -eq $true)
try {
    $prem = Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h
    Check "Premium endpoint unlocked (200)" ($null -ne $prem.message)
} catch { Check "Premium endpoint unlocked (200)" $false }

# 4. Cancel. With a seeded ACTIVE row, it tries PayPal first. Since PayPalSubscriptionId is
#    fake, PayPal cancel will fail -> MUST refuse to downgrade (safety guarantee).
$c = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h
Check "Cancel surfaces PayPal failure (does not silently downgrade)" ($c.success -eq $false -and $c.isSubscribed -eq $true)
Write-Host "      (cancel message: $($c.message))"

# 5. Remove the fake active row so cancel can proceed to the claim-removal path.
& $mysql -u root TubeMailGorillaDB -e "DELETE FROM Subscriptions WHERE PayPalOrderId='SUBSCRIPTION_TEST'"
$login2 = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$h2 = @{ Authorization = "Bearer $($login2.token)" }
$c2 = Invoke-RestMethod -Uri "$base/api/payments/cancel" -Method Post -Headers $h2
Check "Cancel removes claim (fresh token has no premium)" ($c2.success -eq $true -and $c2.isSubscribed -eq $false)

# 6. Verify new token no longer unlocks premium.
$h3 = @{ Authorization = "Bearer $($c2.token)" }
try {
    Invoke-RestMethod -Uri "$base/api/payments/premium" -Headers $h3 | Out-Null
    Check "After cancel premium locked (403)" $false
} catch { Check "After cancel premium locked (403)" ($_.Exception.Response.StatusCode.value__ -eq 403) }

