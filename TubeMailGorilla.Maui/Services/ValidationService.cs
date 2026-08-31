using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TubeMailGorilla.Maui.Services
{
    /// <summary>
    /// Verdict returned by the API's gatekeeper (POST /api/validation/check).
    /// </summary>
    public class ValidationVerdict
    {
        public bool Approved { get; set; }

        /// <summary>Server's message - shown verbatim to the user on denial.</summary>
        public string Reason { get; set; } = "Could not reach the validation service.";

        /// <summary>The server-approved limit for this action (-1 = unlimited).</summary>
        public int Limit { get; set; } = -1;

        public string PlanName { get; set; } = "Free";
    }

    /// <summary>
    /// Talks to the API's ValidationController - the single gatekeeper for all
    /// workloads (extraction, sending, contacts, icebreakers, templates,
    /// blocklist). The app NEVER decides locally what is allowed; it asks and
    /// only proceeds when the server explicitly approves. Any network failure
    /// is treated as a denial (fail closed).
    /// </summary>
    public class ValidationService
    {
        // Action names - MUST match TubeMailGorilla.Application.DTOs.ValidationAction.
        public const string ExtractLeads = "extract_leads";
        public const string SendEmails = "send_emails";
        public const string ViewContacts = "view_contacts";
        public const string GenerateIcebreaker = "generate_icebreaker";
        public const string UseEmailTemplates = "use_email_templates";
        public const string UseBlocklist = "use_blocklist";

        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://api.tubemailgorilla.xyz";
        }

        /// <summary>Asks the API for a green light on the given workload.</summary>
        public async Task<ValidationVerdict> CheckAsync(string action, int requestedAmount = 0)
        {
            try
            {
                var token = Preferences.Get("AuthToken", string.Empty);
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/api/validation/check");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var body = JsonSerializer.Serialize(new { action, requestedAmount });
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // 401/403 etc = not allowed to even ask -> deny.
                    return new ValidationVerdict { Approved = false, Reason = "Your session could not be validated. Please log in again." };
                }

                var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
                return new ValidationVerdict
                {
                    Approved = doc.TryGetProperty("approved", out var approved) && approved.GetBoolean(),
                    Reason = doc.TryGetProperty("reason", out var reason) ? reason.GetString() ?? "Not permitted." : "Not permitted.",
                    Limit = doc.TryGetProperty("limit", out var limit) && limit.TryGetInt32(out var l) ? l : -1,
                    PlanName = doc.TryGetProperty("planName", out var plan) ? plan.GetString() ?? "Free" : "Free"
                };
            }
            catch
            {
                // Fail closed: no server = no permission.
                return new ValidationVerdict
                {
                    Approved = false,
                    Reason = "We couldn't verify your plan right now (the server was unreachable). Please try again."
                };
            }
        }

        /// <summary>Convenience: check + show the server's denial message.</summary>
        public async Task<ValidationVerdict> CheckOrAlertAsync(ContentPage page, string action, int requestedAmount = 0)
        {
            var verdict = await CheckAsync(action, requestedAmount);
            if (!verdict.Approved)
            {
                await page.DisplayAlert("Upgrade required", verdict.Reason, "OK");
            }
            return verdict;
        }
    }
}