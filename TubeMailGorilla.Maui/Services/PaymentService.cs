using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TubeMailGorilla.Maui.Services
{
    /// <summary>
    /// Talks to the subscription/payment endpoints of the API using the
    /// stored AuthToken. Amounts are NEVER sent from the client - pricing is
    /// server-side configuration.
    /// </summary>
    public class PaymentService
    {
            private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PaymentService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://api.tubemailgorilla.xyz";
    }

        private void ApplyAuth()
        {
            var token = Preferences.Get("AuthToken", string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
        }
                public async Task<PricingInfo?> GetPricingAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync($"{_apiBaseUrl}/api/payments/pricing");
                return JsonSerializer.Deserialize<PricingInfo>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The signed-in user's subscription, in customer-friendly terms
        /// (plan name, price, next billing date) — exactly what the website shows.
        /// </summary>
        public async Task<SubscriptionStatusInfo?> GetSubscriptionStatusAsync()
        {
            try
            {
                ApplyAuth();
                var json = await _httpClient.GetStringAsync($"{_apiBaseUrl}/api/payments/status");
                return JsonSerializer.Deserialize<SubscriptionStatusInfo>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Upgrading happens on the website (single payment experience for all
        /// platforms). Opens the pricing page in the system browser.
        /// </summary>
        public string GetUpgradeWebsiteUrl()
        {
            var webUrl = _configuration["ApiSettings:WebAppUrl"] ?? "https://www.tubemailgorilla.xyz";
            return $"{webUrl}/#/subscription";
        }

        private EntitlementInfo? _entitlementsCache;
        private DateTime _entitlementsCachedAt;

        /// <summary>
        /// Fetches the user's entitlements from the API (cached for 60 seconds
        /// so multiple pages can ask without hammering the server). On any
        /// failure it returns the FREE-plan defaults - fail closed.
        /// </summary>
        public async Task<EntitlementInfo> GetEntitlementsAsync()
        {
            if (_entitlementsCache is not null && (DateTime.UtcNow - _entitlementsCachedAt).TotalSeconds < 60)
            {
                return _entitlementsCache;
            }

            try
            {
                ApplyAuth();
                var json = await _httpClient.GetStringAsync($"{_apiBaseUrl}/api/payments/entitlements");
                var entitlements = JsonSerializer.Deserialize<EntitlementInfo>(json, _jsonOptions);
                if (entitlements is not null)
                {
                    _entitlementsCache = entitlements;
                    _entitlementsCachedAt = DateTime.UtcNow;
                    return entitlements;
                }
            }
            catch
            {
                // Fall through to fail-closed defaults.
            }

            return new EntitlementInfo(); // free limits, everything premium locked
        }

        /// <summary>Clears the cached entitlements (e.g. after upgrade/cancel).</summary>
        public void InvalidateEntitlementsCache()
        {
            _entitlementsCache = null;
        }

        /// <summary>
        /// Server-authoritative feature gate: asks the API whether the current
        /// (paying) user may use the Block Contact page. The API's
        /// [Authorize(Policy = "Subscribed")] policy returns 403 for free
        /// users, 200 for subscribers - so any success status means allowed.
        /// </summary>
        public async Task<bool> IsBlocklistAllowedAsync()
        {
            try
            {
                ApplyAuth();
                using var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/payments/features/blocklist");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Network/API failure = fail closed: keep the premium feature locked.
                return false;
            }
        }

        public async Task<bool> IsSubscribedAsync()
        {
            try
            {
                ApplyAuth();
                var json = await _httpClient.GetStringAsync($"{_apiBaseUrl}/api/payments/status");
                var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
                return doc.TryGetProperty("isSubscribed", out var prop) && prop.GetBoolean();
            }
            catch
            {
                return false;
            }
        }

        public async Task<(string? OrderId, string? ApprovalUrl, string? Error)> CreateOrderAsync(string returnUrl, string cancelUrl)
        {
            ApplyAuth();
            var body = JsonSerializer.Serialize(new { returnUrl, cancelUrl });
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/payments/create", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token expired/invalid - forget it so the next launch shows login.
                Preferences.Remove("AuthToken");
                return (null, null, "Your session has expired. Please log out and sign in again.");
            }

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (null, null, $"Could not start checkout ({response.StatusCode}).");
            }

            var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
            var orderId = doc.TryGetProperty("orderId", out var idProp) ? idProp.GetString() : null;
            var approvalUrl = doc.TryGetProperty("approvalUrl", out var urlProp) ? urlProp.GetString() : null;

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(approvalUrl))
            {
                var message = doc.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;
                return (null, null, message ?? "Checkout could not be started.");
            }

            return (orderId, approvalUrl, null);
        }
        public async Task<(bool Success, string? Message)> CaptureOrderAsync(string orderId)
        {
            ApplyAuth();
            var body = JsonSerializer.Serialize(new { orderId });
            using var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/payments/capture", content);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

            var success = doc.TryGetProperty("success", out var successProp) && successProp.GetBoolean();
            var message = doc.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;

            if (success && doc.TryGetProperty("token", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.String)
            {
                StoreFreshToken(tokenProp.GetString());
            }

            return (success, message);
        }

        public async Task<(bool Success, string? Message)> CancelSubscriptionAsync()
        {
            ApplyAuth();
            using var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/payments/cancel", content);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

            var success = doc.TryGetProperty("success", out var successProp) && successProp.GetBoolean();
            var message = doc.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : null;

            if (success && doc.TryGetProperty("token", out var tokenProp) && tokenProp.ValueKind == JsonValueKind.String)
            {
                StoreFreshToken(tokenProp.GetString());
            }

            return (success, message);
        }

        private void StoreFreshToken(string? freshToken)
        {
            if (!string.IsNullOrEmpty(freshToken))
            {
                Preferences.Set("AuthToken", freshToken);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", freshToken);
            }
        }
    }

        public class PricingInfo
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    /// <summary>
    /// Mirrors the API's GET /api/payments/status response — the same
    /// subscription state the website displays.
    /// </summary>
    public class SubscriptionStatusInfo
    {
        public bool IsSubscribed { get; set; }
        public string PlanId { get; set; } = "free";
        public string PlanName { get; set; } = "Free";
        public string? Tagline { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? NextBillingDate { get; set; }
    }

    /// <summary>
    /// Mirrors the API's GET /api/payments/entitlements response - the single
    /// source of truth for what the current plan allows. Defaults represent
    /// the FREE plan (fail closed).
    /// </summary>
    public class EntitlementInfo
    {
        public bool IsSubscribed { get; set; }
        public string PlanId { get; set; } = "free";
        public string PlanName { get; set; } = "Free";

        /// <summary>Max leads per extraction run (-1 = unlimited).</summary>
        public int MaxLeadsPerExtraction { get; set; } = 5;

        /// <summary>Max contacts shown on the contacts page (-1 = unlimited).</summary>
        public int MaxContactsVisible { get; set; } = 5;

        /// <summary>Max recipients per send campaign (-1 = unlimited).</summary>
        public int MaxEmailsPerCampaign { get; set; } = 5;

        public bool IcebreakerEnabled { get; set; }
        public bool EmailTemplatesEnabled { get; set; }
        public bool BlocklistEnabled { get; set; }

        public bool IsUnlimited(int limit) => limit < 0;
    }
}