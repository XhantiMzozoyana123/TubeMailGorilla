$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5076'

# Ensure API is up
try {
    Invoke-WebRequest -Uri "$base/swagger/v1/swagger.json" -TimeoutSec 3 -UseBasicParsing | Out-Null
} catch {
    Write-Host 'API DOWN - restarting...'
    Start-Process dotnet -ArgumentList @('run','--no-build','--urls',"http://localhost:5076") -WorkingDirectory 'C:\Users\Xhanti\source\repos\TubeMailGorillaOfficial\TubeMailGorilla.Api' -WindowStyle Hidden
    Start-Sleep 10
}

$email = "pp-$(Get-Random)@test.com"
Invoke-RestMethod -Uri "$base/api/auth/register" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json' | Out-Null
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -Body "{`"email`":`"$email`",`"password`":`"Abcdef1`"}" -ContentType 'application/json'
$headers = @{ Authorization = "Bearer $($login.token)" }

try {
    $p = Invoke-RestMethod -Uri "$base/api/payments/create" -Method Post -Headers $headers `
        -Body '{"amount":9.99,"currency":"USD","returnUrl":"http://localhost:5076/payments/success","cancelUrl":"http://localhost:5076/payments/cancelled"}' `
        -ContentType 'application/json'
    Write-Host "ORDER CREATED : $($p.orderId)"
    Write-Host "APPROVAL URL  : $($p.approvalUrl)"
} catch {
    Write-Host "FAILED: HTTP $($_.Exception.Response.StatusCode.value__)"
    Write-Host $_.ErrorDetails.Message
}
