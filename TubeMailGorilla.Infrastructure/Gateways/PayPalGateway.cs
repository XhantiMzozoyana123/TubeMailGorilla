using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Interfaces;

namespace TubeMailGorilla.Infrastructure.Gateways;

/// <summary>
/// Infrastructure implementation of <see cref="IPaymentGateway"/> backed by the
/// PayPal SUBSCRIPTIONS API (recurring monthly billing). The billing plan is
/// created ONCE in the PayPal dashboard and pinned via PayPalSettings:PlanId;
/// every checkout subscribes the buyer to that exact plan.
///   - POST /v1/billing/subscriptions                 (buyer approves)
///   - GET  /v1/billing/subscriptions/{id}            (verify state)
///   - POST /v1/billing/subscriptions/{id}/cancel     (stop future charges)
/// </summary>
public class PayPalGateway : IPaymentGateway
{
    private readonly PayPalSettings _settings;

    public PayPalGateway(IOptions<PayPalSettings> settings)
    {
        _settings = settings.Value;
    }

    private HttpClient CreateClient(string accessToken)
    {
        var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return http;
    }

    private string ApiBaseUrl =>
        string.Equals(_settings.Mode, "live", StringComparison.OrdinalIgnoreCase)
            ? "https://api-m.paypal.com/"
            : "https://api-m.sandbox.paypal.com/";

    private async Task<string?> GetAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.Secret))
            return null;

        using var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.Secret}"));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        var response = await http.PostAsync("v1/oauth2/token", content);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
    }

    // ------------------------------------------------------------------
    // IPaymentGateway
    // ------------------------------------------------------------------

    public async Task<SubscriptionCheckoutResult> StartSubscriptionAsync(string returnUrl, string cancelUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.PlanId))
            {
                return new SubscriptionCheckoutResult
                {
                    Success = false,
                    Error = "PayPal PlanId is not configured. Create a billing plan once and pin its id (P-...) in appsettings.json under PayPalSettings:PlanId."
                };
            }

            var token = await GetAccessTokenAsync();
            if (token is null)
            {
                return new SubscriptionCheckoutResult { Success = false, Error = "PayPal authentication failed." };
            }

            using var http = CreateClient(token);

            var payload = new
            {
                plan_id = _settings.PlanId,
                application_context = new
                {
                    brand_name = "TubeMail Gorilla",
                    user_action = "SUBSCRIBE_NOW",
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var response = await http.PostAsync(
                "v1/billing/subscriptions",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"PayPal subscription creation failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var subscriptionId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

            string? approvalUrl = null;
            if (root.TryGetProperty("links", out var links) && links.ValueKind == JsonValueKind.Array)
            {
                foreach (var link in links.EnumerateArray())
                {
                    if (link.TryGetProperty("rel", out var relProp) && relProp.GetString() == "approve"
                        && link.TryGetProperty("href", out var hrefProp))
                    {
                        approvalUrl = hrefProp.GetString();
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(subscriptionId) || string.IsNullOrEmpty(approvalUrl))
            {
                return new SubscriptionCheckoutResult { Success = false, Error = "PayPal returned an incomplete subscription response." };
            }

            return new SubscriptionCheckoutResult { Success = true, PayPalSubscriptionId = subscriptionId, ApprovalUrl = approvalUrl };
        }
        catch (HttpRequestException ex)
        {
            return new SubscriptionCheckoutResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<RemoteSubscriptionState> GetRemoteStatusAsync(string payPalSubscriptionId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (token is null)
                return new RemoteSubscriptionState { Success = false, Error = "PayPal authentication failed." };

            using var http = CreateClient(token);
            var response = await http.GetAsync($"v1/billing/subscriptions/{payPalSubscriptionId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"PayPal status check failed ({(int)response.StatusCode}): {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var state = new RemoteSubscriptionState { Success = true };

            if (root.TryGetProperty("status", out var statusProp))
                state.Status = statusProp.GetString();

            // billing_info.last_payment holds the latest recurring charge.
            if (root.TryGetProperty("billing_info", out var billing)
                && billing.TryGetProperty("last_payment", out var lastPayment)
                && lastPayment.TryGetProperty("amount", out var amountProp))
            {
                if (amountProp.TryGetProperty("value", out var valueProp)
                    && decimal.TryParse(valueProp.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amt))
                {
                    state.LastPaymentAmount = amt;
                }

                if (amountProp.TryGetProperty("currency_code", out var curProp))
                    state.Currency = curProp.GetString();
            }

            return state;
        }
        catch (HttpRequestException ex)
        {
            return new RemoteSubscriptionState { Success = false, Error = ex.Message };
        }
    }

    public async Task<bool> CancelRemoteSubscriptionAsync(string payPalSubscriptionId, string reason)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            if (token is null) return false;

            using var http = CreateClient(token);

            var payload = new { reason };
            var response = await http.PostAsync(
                $"v1/billing/subscriptions/{payPalSubscriptionId}/cancel",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            // PayPal answers 204 No Content on success. 404 means it is already gone.
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
        }
        catch
        {
            return false;
        }
    }
}