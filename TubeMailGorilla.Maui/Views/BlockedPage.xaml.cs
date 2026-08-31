using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class BlockedPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly PaymentService _payments;
    private readonly ValidationService _validator;
    private bool _isUnlocked;

    public BlockedPage()
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _payments = ServiceHelper.GetService<PaymentService>();
        _validator = ServiceHelper.GetService<ValidationService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshAccessAsync();
        if (_isUnlocked)
        {
            await LoadBlockersAsync();
        }
    }

    /// <summary>
    /// Server-authoritative Pro check. The API's [Authorize(Policy = "Subscribed")]
    /// policy decides - the app never trusts a local flag, so cancelling the
    /// subscription locks this page on the very next visit.
    /// </summary>
    private async Task RefreshAccessAsync()
    {
        SetGateBusy(true);
        try
        {
            var verdict = await _validator.CheckAsync(ValidationService.UseBlocklist);
            _isUnlocked = verdict.Approved;
        }
        finally
        {
            SetGateBusy(false);
        }

        ProContent.IsVisible = _isUnlocked;
        ProGate.IsVisible = !_isUnlocked;
    }

    private async void OnUpgradeClicked(object? sender, EventArgs e)
    {
        UpgradeButton.IsEnabled = false;
        try
        {
            // Same single payment experience as SubscriptionPage: checkout on the website.
            await Browser.Default.OpenAsync(_payments.GetUpgradeWebsiteUrl(), BrowserLaunchMode.SystemPreferred);
            await DisplayAlert("Upgrade on our website", "Complete your upgrade in the browser that just opened, then tap \"I've subscribed - check again\" here.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            UpgradeButton.IsEnabled = true;
        }
    }

    private async void OnRecheckClicked(object? sender, EventArgs e)
    {
        _payments.InvalidateEntitlementsCache();
        await RefreshAccessAsync();
        if (_isUnlocked)
        {
            await LoadBlockersAsync();
        }
        else
        {
            await DisplayAlert("Still locked", "We couldn't find an active Pro subscription for your account yet.", "OK");
        }
    }

    // Defense-in-depth: every blocklist action re-verifies before touching data.
    private async Task<bool> EnsureUnlockedAsync()
    {
        if (_isUnlocked) return true;

        var verdict = await _validator.CheckAsync(ValidationService.UseBlocklist);
        _isUnlocked = verdict.Approved;
        if (_isUnlocked)
        {
            ProContent.IsVisible = true;
            ProGate.IsVisible = false;
            return true;
        }

        ProContent.IsVisible = false;
        ProGate.IsVisible = true;
        await DisplayAlert("Pro required", "The contact blocklist is available on the Pro plan.", "OK");
        return false;
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
        if (!await EnsureUnlockedAsync()) return;

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
        if (!await EnsureUnlockedAsync()) return;

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

    private void SetGateBusy(bool busy)
    {
        GateBusyIndicator.IsVisible = busy;
        GateBusyIndicator.IsRunning = busy;
        UpgradeButton.IsEnabled = !busy;
        RecheckButton.IsEnabled = !busy;
    }
}
