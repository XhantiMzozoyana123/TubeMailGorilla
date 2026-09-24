using System.Threading.Tasks;

namespace TubeMailGorilla.Mvc.Services
{
    /// <summary>
    /// Verdict for a workload. In the unlocked edition every verdict is
    /// "approved" - the type is kept so pages can stay identical to the
    /// subscription edition, but there is no server in the decision.
    /// </summary>
    public class ValidationVerdict
    {
        public bool Approved { get; set; } = true;

        /// <summary>Informational only - never shown as a denial anymore.</summary>
        public string Reason { get; set; } = "All features are unlocked.";

        /// <summary>The approved limit for this action (-1 = unlimited).</summary>
        public int Limit { get; set; } = -1;

        public string PlanName { get; set; } = "Unlocked";
    }

    /// <summary>
    /// UNLOCKED EDITION: there is no validation server and no paywall. Every
    /// call is approved locally with an unlimited allowance, so no feature can
    /// ever be locked out. The action-name constants are kept because the
    /// pages still pass them.
    /// </summary>
    public class ValidationService
    {
        // Action names - kept for call-site parity with the subscription edition.
        public const string ExtractLeads = "extract_leads";
        public const string BulkExtractLeads = "bulk_extract_leads";
        public const string SendEmails = "send_emails";
        public const string ViewContacts = "view_contacts";
        public const string GenerateIcebreaker = "generate_icebreaker";
        public const string UseEmailTemplates = "use_email_templates";
        public const string UseBlocklist = "use_blocklist";

        /// <summary>Always approves - the unlocked edition has no gatekeeper.</summary>
        public Task<ValidationVerdict> CheckAsync(string action, int requestedAmount = 0)
            => Task.FromResult(new ValidationVerdict());    }
}
