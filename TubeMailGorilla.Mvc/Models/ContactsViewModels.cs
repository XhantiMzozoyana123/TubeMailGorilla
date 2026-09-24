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

    public bool IsNew => Contact.Id == 0;

    public string? StatusMessage { get; set; }
    public string? Error { get; set; }
}
