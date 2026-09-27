using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Models;

/// <summary>View model for the Contacts list page (ContactsPage).</summary>
public class ContactsViewModel
{
    /// <summary>Contacts matching the current search + sort (already filtered).</summary>
    public List<EmailContact> Contacts { get; set; } = new();

    public string Query { get; set; } = string.Empty;

    /// <summary>0 = Newest first, 1 = Name (A-Z), 2 = Email (A-Z) - same order as the MAUI picker.</summary>
    public int Sort { get; set; }

    /// <summary>Every contact in the store (drives empty states and the count label).</summary>
    public int TotalCount { get; set; }

    public string CountLabel =>
        TotalCount == 0 ? "0 leads"
        : Contacts.Count == TotalCount ? $"{TotalCount} {(TotalCount == 1 ? "lead" : "leads")}"
        : $"{Contacts.Count} of {TotalCount} leads";

    public string EmptyMessage =>
        TotalCount == 0
            ? "No contacts yet. Extract some leads from the Extract page."
            : $"No leads match \u201C{Query}\u201D. Clear the search box to see all {TotalCount}.";
}

/// <summary>View model for the contact details page (ContactDetailsPage).</summary>
public class ContactDetailsViewModel
{
    public EmailContact Contact { get; set; } = new();

    /// <summary>Saved AI icebreakers (Openers) for this lead.</summary>
    public List<Opener> Openers { get; set; } = new();

    /// <summary>
    /// The lead's video snapshots, index-aligned with the seek position each
    /// frame was captured at. Empty when the lead has none.
    /// </summary>
    public List<VideoSnapshotItem> Snapshots { get; set; } = new();

    /// <summary>Latest "areas of improvement" analysis, or null when none was run yet.</summary>
    public string? VideoAdvice { get; set; }

    public bool IsNew => Contact.Id == 0;

    /// <summary>
    /// One snapshot as the details page shows it: the base64 JPEG plus the
    /// transcript timestamp and seconds it was captured at. Kept separate from
    /// the stored <see cref="EmailContact"/> shape so the view has exactly what
    /// it binds to.
    /// </summary>
    public class VideoSnapshotItem
    {
        public VideoSnapshotItem(string image, string timestampLabel, double seconds, string? videoUrl)
        {
            Image = image;
            TimestampLabel = timestampLabel;
            Seconds = seconds;
            VideoUrl = videoUrl;
        }

        /// <summary>Base64 JPEG data (no data-URI prefix).</summary>
        public string Image { get; }

        /// <summary>"00:00:10" - when in the video this frame came from.</summary>
        public string TimestampLabel { get; }

        /// <summary>The same moment in seconds, used to seek the source video.</summary>
        public double Seconds { get; }

        /// <summary>
        /// The source video, with the seek position applied, or null when unknown.
        /// Built on demand so the view can just print the href.
        /// </summary>
        public string? VideoUrl { get; }

        /// <summary>Data URI for an &lt;img src&gt;.</summary>
        public string DataUri => "data:image/jpeg;base64," + Image;
    }

    public string? StatusMessage { get; set; }
    public string? Error { get; set; }
}
