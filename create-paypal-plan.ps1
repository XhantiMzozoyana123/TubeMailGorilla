<#
    create-paypal-plan.ps1
    ----------------------
    Creates the TubeMailGorilla Premium monthly billing plan at PayPal and
    stores its id (P-...) into TubeMailGorilla.Api/appsettings.json
    under PayPalSettings:PlanId.

    PREREQUISITE: the "Subscriptions" feature must be ENABLED for the REST app
    (developer.paypal.com > Apps & Credentials) or the API returns an empty 404.

    Usage:
        powershell -ExecutionPolicy Bypass -File create-paypal-plan.ps1
#>

$root     = Split-Path -Parent $PSScriptRoot
$apiJson  = Join-Path $root 'TubeMailGorilla.Api\appsettings.json'
$base     = 'https://api-m.sandbox.paypal.com'

# Credentials + price read from appsettings so there is a single source of truth.
$cfg = Get-Content $apiJson -Raw | ConvertFrom-Json
$clientId = $cfg.PayPalSettings.ClientId
$secret   = $cfg.PayPalSettings.Secret
$amount   = $cfg.Pricing.Amount
$currency = $cfg.Pricing.Currency

if (-not $clientId -or -not $secret) { Write-Error 'ClientId/Secret missing in appsettings.json' ; exit 1 }

function Post($uri, $json, $token) {
    $body = [Text.Encoding]::UTF8.GetBytes($json)
    return Invoke-WebRequest -Uri "$base$uri" -Method Post -Headers @{ Authorization = "Bearer $token"; 'Content-Type'='application/json' } -Body $body -UseBasicParsing
}
function Get-Token {
    $pair = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("$clientId`:$secret"))
    $resp = Invoke-RestMethod -Uri "$base/v1/oauth2/token" -Method Post -Headers @{ Authorization = "Basic $pair" } -Body @{ grant_type='client_credentials' }
    return $resp.access_token
}

$token = Get-Token
Write-Host "Access token OK (len $($token.Length))"

# 1) Product (idempotent: reuse if it already exists).
$productId = $null
$prodPayload = @{ name='TubeMailGorilla Premium'; description='TubeMailGorilla Premium subscription'; type='SERVICE' } | ConvertTo-Json
try {
    $pr = Post '/v1/catalogs/products' $prodPayload $token
    $productId = ($pr.Content | ConvertFrom-Json).id
    Write-Host "Product created: $productId"
} catch {
    # 422 => already exists; look it up (Catalog API, not billing).
    if ([int]$_.Exception.Response.StatusCode -eq 422) {
        $list = Invoke-RestMethod -Uri "$base/v1/catalogs/products?page_size=20" -Headers @{ Authorization = "Bearer $token" }
        $productId = ($list.products | Where-Object { $_.name -eq 'TubeMailGorilla Premium' } | Select-Object -First 1).id
        Write-Host "Reusing existing product: $productId"
    } else {
        Write-Host ("PayPal product step failed: " + $_.Exception.Message)
        exit 1
    }
}

# 2) Monthly plan.
$planPayload = @{
    product_id = $productId
    name       = 'TubeMailGorilla Premium - Monthly'
    status     = 'ACTIVE'
    billing_cycles = @(@{
        frequency = @{ interval_unit='MONTH'; interval_count=1 }
        tenure_type='REGULAR'
        sequence=1
        total_cycles=0
        pricing_scheme = @{ fixed_price = @{ value = ([math]::Round($amount,2)).ToString('0.00'); currency_code=$currency } }
    })
    payment_preferences = @{ auto_bill_outstanding=$true }
} | ConvertTo-Json -Depth 6

$pl = Post '/v1/billing/plans' $planPayload $token
$planId = ($pl.Content | ConvertFrom-Json).id
Write-Host "Plan created: $planId ($amount $currency / month)"

# 3) Write the plan id back into appsettings.json.
$cfg.PayPalSettings.PlanId = $planId
$cfg | ConvertTo-Json -Depth 10 | Set-Content $apiJson -Encoding utf8
Write-Host "PlanId saved to $apiJson"