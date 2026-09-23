using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

/// <summary>
/// Blocklist manager. The unlocked edition has no Pro gate: this page is
/// always fully available.
/// </summary>
public partial class BlockedPage : ContentPage
{
    private readonly DatabaseService _db;

    public BlockedPage()
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBlockersAsync();
    }

    private async Task LoadBlockersAsync()
    {
        try
        {
            var blockers = await _db.GetBlockersAsync();
            BlockedList.ItemsSource = blockers;
            EmptyBlockedListLabel.IsVisible = blockers.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load blocked emails: {ex.Message}", "OK");
            BlockedList.ItemsSource = new List<Blocker>();
            EmptyBlockedListLabel.IsVisible = true;
            EmptyBlockedListLabel.Text = "Failed to load blocked emails.";
        }
    }

    private async void OnAddBlockerClicked(object? sender, EventArgs e)
    {
        var email = (BlockedEmailEntry.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Email required", "Please enter an email address to block.", "OK");
            return;
        }

        try
        {
            var blocker = new Blocker
            {
                BlockedEmail = email,
                CreatedAt = DateTime.Now
            };
            await _db.AddBlockerAsync(blocker);
            BlockedEmailEntry.Text = string.Empty;
            await LoadBlockersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not block email: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteBlockerClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var blocker = button?.BindingContext as Blocker;
        if (blocker == null) return;

        var confirm = await DisplayAlert(
            "Unblock email?",
            $"Remove \"{blocker.BlockedEmail}\" from the blocklist?",
            "Unblock", "Cancel");

        if (!confirm) return;

        try
        {
            await _db.RemoveBlockerAsync(blocker.Id);
            await LoadBlockersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not unblock email: {ex.Message}", "OK");
        }
    }
}
