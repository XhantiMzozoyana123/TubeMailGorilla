using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

/// <summary>
/// The lead list. Every contact is loaded once and then filtered / sorted in
/// memory, so typing in the search box or changing the sort order is instant.
/// The list itself lives in a bounded row (see ContactsPage.xaml), so it scrolls
/// and virtualises no matter how many leads were extracted.
/// </summary>
public partial class ContactsPage : ContentPage
{
    private const string SortNewestFirst = "Newest first";
    private const string SortNameAsc = "Name (A-Z)";
    private const string SortEmailAsc = "Email (A-Z)";

    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly ValidationService _validator;

    /// <summary>Every contact in the database - the source for search and sort.</summary>
    private readonly List<EmailContact> _allContacts = new();

    /// <summary>How many contacts the current plan may show (int.MaxValue = all).</summary>
    private int _visibleLimit = int.MaxValue;

    private bool _isLoading;
    private bool _isGeneratingIcebreakers;
    private bool _isNavigating;
    private int _renderLimit = int.MaxValue;
    private List<EmailContact> _lastMatches = new();

    public ContactsPage()
    {
        InitializeComponent();

        _db = ServiceHelper.GetService<DatabaseService>();
        _ai = ServiceHelper.GetService<AIService>();
        _validator = ServiceHelper.GetService<ValidationService>();

        SortPicker.ItemsSource = new List<string> { SortNewestFirst, SortNameAsc, SortEmailAsc };
        SortPicker.SelectedIndex = 0; // also fires OnSortChanged, which is safe with an empty list
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Unlocked edition: every feature (and every contact) is available.
        await LoadContactsAsync();
    }

    private async Task LoadContactsAsync()
    {
        if (_isLoading) return; // OnAppearing can fire again before a slow load finishes
        _isLoading = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            var contacts = await _db.GetContactsAsync();
            _allContacts.Clear();
            _allContacts.AddRange(contacts);

            // GATEKEEPER: the service decides how many contacts may be viewed.
            // The unlocked edition always approves, with no limit.
            var verdict = await _validator.CheckAsync(ValidationService.ViewContacts, _allContacts.Count);
            _visibleLimit = verdict.Approved && verdict.Limit < 0 ? int.MaxValue : Math.Max(verdict.Limit, 0);

            var viewable = GetViewableContacts().Count;
            var hidden = _allContacts.Count - viewable;

            FreePlanBanner.IsVisible = hidden > 0;
            if (hidden > 0)
            {
                FreePlanBanner.Text = !verdict.Approved
                    ? $"{verdict.Reason} Showing the first {viewable}."
                    : $"Free plan: showing {viewable} of {_allContacts.Count} contacts. Upgrade to Pro on the Subscription tab to see all of them.";
            }

            // Render progressively so 60+ rows appear fast: first paint then the rest.
            RenderContacts(initialLimit: 25);
            if (GetViewableContacts().Count > 25)
            {
                await Task.Delay(50);
                ApplySearchAndSort();
            }
        }
        catch (Exception ex)
        {
            _allContacts.Clear();
            _visibleLimit = int.MaxValue;
            ContactsList.ItemsSource = new List<EmailContact>();
            ContactsCountLabel.Text = "0 leads";
            EmptyContactsLabel.Text = "Could not load contacts. Restart the app and try again.";
            EmptyContactsLabel.IsVisible = true;
            await DisplayAlert("Error", $"Could not load contacts: {ex.Message}", "OK");
        }
        finally
        {
            _isLoading = false;
            LoadingIndicator.IsVisible = false;
        }
    }
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => ApplySearchAndSort();

    private void OnSortChanged(object? sender, EventArgs e) => ApplySearchAndSort();

    /// <summary>The contacts the current plan is allowed to show.</summary>
    private List<EmailContact> GetViewableContacts() => _visibleLimit == int.MaxValue
        ? _allContacts.ToList()
        : _allContacts.Take(_visibleLimit).ToList();

    /// <summary>
    /// Applies the current search text and sort order to the loaded contacts and
    /// refreshes the list, the "x of y leads" counter and the empty state.
    /// </summary>
    private void ApplySearchAndSort() => RenderContacts(initialLimit: int.MaxValue);

    /// <summary>
    /// Filters + sorts the viewable contacts and binds them. The optional
    /// initialLimit renders just the first N rows so a 60-lead scrape paints
    /// instantly; RemainingItemsThresholdReached then appends the remainder.
    /// </summary>
    private void RenderContacts(int initialLimit)
    {
        _renderLimit = initialLimit;
        var viewable = GetViewableContacts();
        var query = SearchBarControl.Text?.Trim() ?? string.Empty;

        var matches = query.Length == 0
            ? viewable
            : viewable.Where(c => Matches(c, query)).ToList();

        _lastMatches = SortContacts(matches).ToList();
        var shown = _renderLimit >= _lastMatches.Count
            ? _lastMatches
            : _lastMatches.Take(_renderLimit).ToList();
        ContactsList.ItemsSource = shown;

        ContactsCountLabel.Text = shown.Count == viewable.Count
            ? $"{viewable.Count} {(viewable.Count == 1 ? "lead" : "leads")}"
            : $"{shown.Count} of {viewable.Count} leads";

        var hasNoContacts = _allContacts.Count == 0;
        EmptyContactsLabel.IsVisible = hasNoContacts || shown.Count == 0;
        EmptyContactsLabel.Text = hasNoContacts
            ? "No contacts yet. Extract some leads from the Extract page."
            : $"No leads match \"{query}\". Clear the search box to see all {viewable.Count}.";
    }

    private static bool Matches(EmailContact contact, string query) =>
        (contact.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
        || contact.Email.Contains(query, StringComparison.OrdinalIgnoreCase)
        || (contact.Channel?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);

    /// <summary>Orders the given contacts the way the sort picker asks for.</summary>
    private IEnumerable<EmailContact> SortContacts(IEnumerable<EmailContact> contacts) => SortPicker.SelectedIndex switch
    {
        1 => contacts.OrderBy(c => string.IsNullOrWhiteSpace(c.Name) ? 1 : 0)
                     .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase),
        2 => contacts.OrderBy(c => c.Email, StringComparer.OrdinalIgnoreCase),
        // Default: newest first, so a lead that was just scraped is always on top.
        _ => contacts.OrderByDescending(c => c.ExtractedAt).ThenByDescending(c => c.Id)
    };
    private async void OnContactSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (ContactsList.SelectedItem is not EmailContact contact) return;

        ContactsList.SelectedItem = null;
        await OpenContactAsync(contact);
    }

    /// <summary>Row tap (also used by the swipe-reveal chevron area).</summary>
    private async void OnContactTapped(object? sender, TappedEventArgs e)
    {
        var contact = ((sender as Element)?.BindingContext as EmailContact)
            ?? (e.Parameter as EmailContact);
        if (contact is null) return;
        await OpenContactAsync(contact);
    }

    private async Task OpenContactAsync(EmailContact contact)
    {
        if (_isNavigating) return; // double-taps on dense rows push two pages
        _isNavigating = true;
        try
        {
            await Navigation.PushAsync(new ContactDetailsPage(contact));
        }
        finally
        {
            _isNavigating = false;
        }
    }

    /// <summary>Appends the next chunk of already-sorted matches when the user nears the end.</summary>
    private void OnRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        if (_renderLimit >= _lastMatches.Count) return;
        _renderLimit = Math.Min(_lastMatches.Count, _renderLimit + 25);
        ContactsList.ItemsSource = _lastMatches.Take(_renderLimit).ToList();
    }

    private async void OnAddContactClicked(object? sender, EventArgs e)
    {
        var contact = new EmailContact { ExtractedAt = DateTime.Now };
        await Navigation.PushAsync(new ContactDetailsPage(contact));
    }

    /// <summary>Swipe-to-delete from the row's right swipe items.</summary>
    private async void OnDeleteSwipeInvoked(object? sender, EventArgs e)
    {
        if ((sender as SwipeItem)?.BindingContext is not EmailContact contact) return;
        await DeleteContactAsync(contact);
    }

    private async Task DeleteContactAsync(EmailContact contact)
    {
        var confirm = await DisplayAlert(
            "Delete contact?",
            $"Remove \"{contact.Email}\" from your contacts?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteContactAsync(contact.Id);
            await LoadContactsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete contact: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteAllContactsClicked(object? sender, EventArgs e)
    {
        if (_allContacts.Count == 0)
        {
            await DisplayAlert("Nothing to delete", "You have no contacts to delete.", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Delete All Contacts?",
            $"Are you sure you want to delete ALL {_allContacts.Count} contacts? This cannot be undone.",
            "Delete All", "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteAllContactsAsync();
            await LoadContactsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete contacts: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Bulk-generates a personalized icebreaker (Opener) for every lead currently
    /// listed (i.e. after the search / sort above). Runs sequentially to respect LLM
    /// rate limits and reports progress on the button itself.
    /// </summary>
    private async void OnGenerateIcebreakersClicked(object? sender, EventArgs e)
    {
        if (_isGeneratingIcebreakers) return;

        // Unlocked edition: AI icebreakers are always available.
        var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.GenerateIcebreaker);
        if (!verdict.Approved) return;

        var targets = (ContactsList.ItemsSource as IEnumerable<EmailContact>)?.ToList() ?? new List<EmailContact>();
        if (targets.Count == 0)
        {
            await DisplayAlert("No contacts", "Extract some leads first, then generate icebreakers.", "OK");
            return;
        }

        var confirm = await DisplayAlert(
            "Generate Icebreakers",
            $"Create an AI-personalized first line for each of the {targets.Count} leads shown?\n\nThis can take a moment.",
            "Generate", "Cancel");
        if (!confirm) return;

        _isGeneratingIcebreakers = true;
        GenerateIcebreakersButton.IsEnabled = false;

        int created = 0, failed = 0, consecutiveFailures = 0;

        try
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var contact = targets[i];
                GenerateIcebreakersButton.Text = $"✨ Generating… {i + 1}/{targets.Count}";

                try
                {
                    var icebreaker = await _ai.GenerateIcebreakerAsync(contact);
                    if (string.IsNullOrWhiteSpace(icebreaker))
                    {
                        failed++;
                        // A timeout/unreachable-server failure will fail for every
                        // remaining lead too - bail out instead of burning the full
                        // inference timeout on each one.
                        if (++consecutiveFailures >= 3)
                            break;
                        continue;
                    }

                    consecutiveFailures = 0;
                    await _db.SaveOpenerAsync(new Opener
                    {
                        EmailerId = contact.Id,
                        Text = icebreaker,
                        CreatedAt = DateTime.Now
                    });
                    created++;
                }
                catch
                {
                    failed++;
                    if (++consecutiveFailures >= 3)
                        break;
                }
            }
        }
        finally
        {
            _isGeneratingIcebreakers = false;
            GenerateIcebreakersButton.IsEnabled = true;
            GenerateIcebreakersButton.Text = "✨ Generate Icebreakers";
        }

        var summary = $"Created {created} icebreaker{(created == 1 ? "" : "s")}";
        if (failed > 0) summary += $"\n{failed} failed (the AI service timed out or was unreachable - check your connection and try again)";
        if (consecutiveFailures >= 3) summary += "\nStopped early after 3 consecutive failures.";
        await DisplayAlert("Done", summary, "OK");
    }
}
