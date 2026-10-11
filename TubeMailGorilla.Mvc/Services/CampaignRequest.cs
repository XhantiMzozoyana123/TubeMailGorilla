namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Snapshot of everything a campaign send needs, captured at enqueue time.
/// The worker re-reads live contact/sender rows by id, but the composed copy
/// travels with the job so editing the draft mid-send cannot corrupt a run.
/// </summary>
public sealed class CampaignRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<int> ContactIds { get; set; } = new();
    public bool AllowAccountRotation { get; set; }
    public int DefaultSenderId { get; set; }
    public bool AllowMessageRotation { get; set; }
    public List<Models.MessageVariation> Variations { get; set; } = new();
}
