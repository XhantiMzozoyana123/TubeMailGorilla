using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class SettingsPage : ContentPage
{
        private readonly DatabaseService _db;

    public SettingsPage()
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        GmailOnlySwitch.IsToggled = SendSettings.ExtractGmailOnly;
        ValidateEmailsSwitch.IsToggled = SendSettings.ExtractValidateEmails;
        PageLimitEntry.Text = "5";
        UpdateSubscriptionInfo();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        EnsurePickerPopulated();
        await LoadAccountsAsync();
        await LoadParametersAsync();
    }

    /// <summary>
    /// Loads the user's email shortcodes from SQLite into the list, and fills
    /// the "Add something…" picker with everything a lead's data can provide.
    /// </summary>
    private async Task LoadParametersAsync()
    {
        try
        {
            var parameters = await _db.GetMessageParametersAsync();
            ParametersList.ItemsSource = parameters;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load shortcodes: {ex.Message}", "OK");
        }
    }
    // Friendly picker options -> internal data field.
    private static readonly (string Label, string Field)[] AddableFields =
    {
        ("Their first name", "first-name"),
        ("Their last name", "last-name"),
        ("Their full name", "name"),
        ("Their email address", "email"),
        ("Their YouTube channel name", "channel-name"),
        ("Their latest video title", "video-title"),
        ("Their video description", "video-description"),
        ("An AI icebreaker first line", "icebreaker"),
    };

    private void EnsurePickerPopulated()
    {
        if (NewFieldPicker.ItemsSource is { Count: > 0 }) return;
        foreach (var (label, _) in AddableFields)
            NewFieldPicker.Items.Add(label);
    }

    private async void OnAddParameterClicked(object? sender, EventArgs e)
    {
        EnsurePickerPopulated();
        var index = NewFieldPicker.SelectedIndex;
        if (index < 0 || index >= AddableFields.Length)
        {
            await DisplayAlert("Pick one", "Choose what you want to insert from the dropdown first.", "OK");
            return;
        }

        var (_, field) = AddableFields[index];

        try
        {
            var existing = await _db.GetMessageParametersAsync();

            // One shortcode per data type keeps things simple - if it already
            // exists just tell the user it's ready to use.
            if (existing.Any(p => p.Field.Equals(field, StringComparison.OrdinalIgnoreCase)))
            {
                var already = existing.First(p => p.Field.Equals(field, StringComparison.OrdinalIgnoreCase));
                await DisplayAlert("Already added",
                    $"That one is already in your emails as [{already.Token}].", "OK");
                return;
            }

            // Suggest a sensible default token based on the field name.
            var token = field.Replace('-', '_');
            await _db.SaveMessageParameterAsync(new MessageParameter { Token = token, Field = field });
            NewFieldPicker.SelectedItem = null;
            await LoadParametersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not add shortcode: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteParameterClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.BindingContext is not MessageParameter parameter || parameter.Id == 0) return;

        try
        {
            await _db.DeleteMessageParameterAsync(parameter.Id);
            await LoadParametersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete shortcode: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Renames the currently tapped shortcode (e.g. turn [f_name] into
    /// [first_name]). Tap a row in the list to select it, type the new
    /// spelling, then tap Rename.
    /// </summary>
    private async void OnRenameParameterClicked(object? sender, EventArgs e)
    {
        if (ParametersList.SelectedItem is not MessageParameter selected || selected.Id == 0)
        {
            await DisplayAlert("Pick a shortcode", "Tap a shortcode in the list above to select it, then type its new spelling.", "OK");
            return;
        }

        var newToken = (CustomTokenEntry.Text ?? string.Empty).Trim().Trim('[', ']');
        if (string.IsNullOrWhiteSpace(newToken))
        {
            await DisplayAlert("Missing name", "Type the new shortcode spelling first.", "OK");
            return;
        }

        try
        {
            selected.Token = newToken;
            await _db.SaveMessageParameterAsync(selected);
            CustomTokenEntry.Text = string.Empty;
            await LoadParametersAsync();
            await DisplayAlert("Renamed", $"Use [{newToken}] in your emails.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not rename shortcode: {ex.Message}", "OK");
        }
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var accounts = await _db.GetAllSendersAsync();
            AccountsList.ItemsSource = accounts;
            EmptyAccountsLabel.IsVisible = accounts.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load accounts: {ex.Message}", "OK");
            EmptyAccountsLabel.IsVisible = true;
            EmptyAccountsLabel.Text = "Failed to load accounts.";
        }
    }

    private void UpdateSubscriptionInfo()
    {
        var current = Subscriptions.Current;
        if (current is null)
        {
            SubscriptionLabel.Text = "Current plan: Free";
            SubscriptionDescLabel.Text = "Up to 100 leads, 50 emails/month";
        }
        else
        {
            SubscriptionLabel.Text = $"Current plan: {current.Name} ({current.Price})";
            SubscriptionDescLabel.Text = current.Description;
        }
    }

    private void OnGmailOnlyToggled(object? sender, ToggledEventArgs e)
        => SendSettings.ExtractGmailOnly = e.Value;

    private void OnValidateEmailsToggled(object? sender, ToggledEventArgs e)
        => SendSettings.ExtractValidateEmails = e.Value;

    private async void OnAddAccountClicked(object? sender, EventArgs e)
        => await Navigation.PushAsync(new SenderDetailsPage(new Sender()));

    private async void OnAccountSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not Sender selectedSender) return;
        AccountsList.SelectedItem = null;
        await Navigation.PushAsync(new SenderDetailsPage(selectedSender));
    }

    private async void OnDeleteAccountClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var account = button?.BindingContext as Sender;
        if (account == null) return;

        var confirm = await DisplayAlert(
            "Delete account?",
            $"Remove \"{account.EmailAddress}\" from your accounts?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteSenderAsync(account.Id);
            await LoadAccountsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete account: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Log out",
            "Are you sure you want to log out?",
            "Log Out", "Cancel");

        if (!confirm) return;

        try
        {
            // Clear server-side session / local auth token
            var authService = ServiceHelper.GetService<AuthService>();
            await authService.LogoutAsync(); // removes the AuthToken preference
        }
        catch
        {
            // Even if the service call fails, clear locally so the user
            // is never stuck in a logged-in state.
            Preferences.Remove("AuthToken");
        }

        // Clear app-specific session data as well
        SendSettings.ClearSession();

        // Show a fresh login screen by opening a brand-new window (reliable
        // on Windows) and closing the current one.
        App.ShowLoginScreen();
    }
}