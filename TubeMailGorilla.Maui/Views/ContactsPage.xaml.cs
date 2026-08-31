using System.Collections.ObjectModel;
using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class ContactsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly PaymentService _payments;
    private readonly ValidationService _validator;
    private readonly List<EmailContact> _allContacts = new();
    private EntitlementInfo _entitlements = new();
    private int _visibleLimit = 5;
    private bool _isGeneratingIcebreakers;

    public ContactsPage()
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _ai = ServiceHelper.GetService<AIService>();
        _payments = ServiceHelper.GetService<PaymentService>();
        _validator = ServiceHelper.GetService<ValidationService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Server-authoritative plan limits (fail closed to FREE limits).
        _entitlements = await _payments.GetEntitlementsAsync();

        // Icebreaker is Pro-only: hide the button and show the upsell instead.
        GenerateIcebreakersButton.IsVisible = _entitlements.IcebreakerEnabled;
        IcebreakerProLabel.IsVisible = !_entitlements.IcebreakerEnabled;

        await LoadContactsAsync();
    }

    private async Task LoadContactsAsync()
    {
        try
        {
            _allContacts.Clear();
            _allContacts.AddRange(await _db.GetContactsAsync());

            // GATEKEEPER: the API decides how many contacts may be viewed.
            var verdict = await _validator.CheckAsync(ValidationService.ViewContacts, _allContacts.Count);
            _visibleLimit = verdict.Approved && verdict.Limit < 0 ? int.MaxValue : Math.Max(verdict.Limit, 0);
            var maxVisible = _visibleLimit;

            var visible = _allContacts.Take(maxVisible).ToList();
            var hidden = _allContacts.Count - visible.Count;

            ContactsList.ItemsSource = visible.ToList();
            EmptyContactsLabel.IsVisible = _allContacts.Count == 0;

            FreePlanBanner.IsVisible = hidden > 0;
            if (hidden > 0)
            {
                FreePlanBanner.Text = !verdict.Approved
                    ? $"{verdict.Reason} Showing the first {visible.Count}."
                    : $"Free plan: showing {visible.Count} of {_allContacts.Count} contacts. Upgrade to Pro on the Subscription tab to see all of them.";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load contacts: {ex.Message}", "OK");
            ContactsList.ItemsSource = new List<EmailContact>();
            EmptyContactsLabel.IsVisible = true;
            EmptyContactsLabel.Text = "Failed to load contacts. Pull to refresh or restart the app.";
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim().ToLowerInvariant() ?? string.Empty;
        var searchable = GetVisibleContacts();

        if (string.IsNullOrEmpty(query))
        {
            ContactsList.ItemsSource = searchable;
            return;
        }

        ContactsList.ItemsSource = searchable
            .Where(c => (c.Name?.ToLowerInvariant().Contains(query) ?? false)
                     || c.Email.ToLowerInvariant().Contains(query)
                     || (c.Channel?.ToLowerInvariant().Contains(query) ?? false))
            .ToList();

        EmptyContactsLabel.IsVisible = _allContacts.Count == 0;
    }

    /// <summary>The contacts the current plan is allowed to show.</summary>
    private List<EmailContact> GetVisibleContacts()
    {
        return _visibleLimit == int.MaxValue
            ? _allContacts.ToList()
            : _allContacts.Take(_visibleLimit).ToList();
    }

    private async void OnContactSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (ContactsList.SelectedItem is not EmailContact contact) return;

        ContactsList.SelectedItem = null;
        await Navigation.PushAsync(new ContactDetailsPage(contact));
    }

    private async void OnAddContactClicked(object? sender, EventArgs e)
    {
        var contact = new EmailContact { ExtractedAt = DateTime.Now };
        await Navigation.PushAsync(new ContactDetailsPage(contact));
    }

    private async void OnDeleteContactClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var contact = button?.BindingContext as EmailContact;

        if (contact == null) return;

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
            "Are you sure you want to delete ALL contacts? This cannot be undone.",
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
    /// Bulk-generates a personalized icebreaker (Opener) for every contact in
    /// the current list. Runs sequentially to respect LLM rate limits and
    /// reports progress on the button itself.
    /// </summary>
    private async void OnGenerateIcebreakersClicked(object? sender, EventArgs e)
    {
        if (_isGeneratingIcebreakers) return;

        // GATEKEEPER: the API must approve AI usage before any LLM calls.
        var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.GenerateIcebreaker);
        if (!verdict.Approved)
        {
            GenerateIcebreakersButton.IsVisible = false;
            IcebreakerProLabel.IsVisible = true;
            return;
        }

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

        int created = 0, failed = 0;

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
                        continue;
                    }

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
        if (failed > 0) summary += $"\n{failed} failed (check your connection/API key and try again)";
        await DisplayAlert("Done", summary, "OK");
    }
}