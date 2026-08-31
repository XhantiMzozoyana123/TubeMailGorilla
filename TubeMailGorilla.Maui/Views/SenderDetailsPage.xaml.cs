using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class SenderDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly Sender _sender;

    public SenderDetailsPage(Sender sender)
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _sender = sender;
        NameEntry.Text = sender.Name;
        EmailEntry.Text = sender.EmailAddress;
        SmtpHostEntry.Text = sender.SmtpHost ?? string.Empty;
        SmtpPortEntry.Text = sender.SmtpPort > 0 ? sender.SmtpPort.ToString() : "587";
        SmtpUserEntry.Text = sender.SmtpUser ?? string.Empty;
        SmtpPasswordEntry.Text = sender.SmtpPassword ?? string.Empty;
        IsActiveSwitch.IsToggled = sender.IsActive;
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        _sender.Name = NameEntry.Text?.Trim() ?? string.Empty;
        _sender.EmailAddress = (EmailEntry.Text ?? string.Empty).Trim();
        _sender.SmtpHost = string.IsNullOrWhiteSpace(SmtpHostEntry.Text) ? null : SmtpHostEntry.Text.Trim();
        _sender.SmtpPort = int.TryParse(SmtpPortEntry.Text, out var port) && port > 0 ? port : 587;
        _sender.SmtpUser = string.IsNullOrWhiteSpace(SmtpUserEntry.Text) ? null : SmtpUserEntry.Text.Trim();
        _sender.SmtpPassword = string.IsNullOrWhiteSpace(SmtpPasswordEntry.Text) ? null : SmtpPasswordEntry.Text;
        _sender.IsActive = IsActiveSwitch.IsToggled;

        if (string.IsNullOrWhiteSpace(_sender.EmailAddress))
        {
            await DisplayAlert("Email required", "Please enter an email address.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_sender.Name))
        {
            await DisplayAlert("Name required", "Please enter an account name.", "OK");
            return;
        }

        try
        {
            await _db.SaveSenderAsync(_sender);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not save account: {ex.Message}", "OK");
        }
    }

    private async void OnCancel(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnIsActiveToggled(object? sender, ToggledEventArgs e)
    {
        // The IsActive value is saved in OnSave
    }
}
