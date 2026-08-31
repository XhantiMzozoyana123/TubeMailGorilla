using System.Collections.Generic;
using System.Linq;
using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class EmailTemplatesPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly PaymentService _payments;
    private readonly ValidationService _validator;
    private List<EmailTemplate> _templates = new();

    public EmailTemplatesPage()
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
        if (ProContent.IsVisible)
        {
            await LoadTemplatesAsync();
        }
    }

    /// <summary>
    /// Server-authoritative Pro check via GET /api/payments/entitlements.
    /// Never trusts a local flag, so cancelling locks this page immediately.
    /// </summary>
    private async Task RefreshAccessAsync()
    {
        SetGateBusy(true);
        try
        {
            var verdict = await _validator.CheckAsync(ValidationService.UseEmailTemplates);
            ProContent.IsVisible = verdict.Approved;
            ProGate.IsVisible = !verdict.Approved;
        }
        finally
        {
            SetGateBusy(false);
        }
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
        if (ProContent.IsVisible)
        {
            await LoadTemplatesAsync();
        }
        else
        {
            await DisplayAlert("Still locked", "We couldn't find an active Pro subscription for your account yet.", "OK");
        }
    }

    private void SetGateBusy(bool busy)
    {
        GateBusyIndicator.IsVisible = busy;
        GateBusyIndicator.IsRunning = busy;
        UpgradeButton.IsEnabled = !busy;
        RecheckButton.IsEnabled = !busy;
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            _templates = await _db.GetTemplatesAsync();
            TemplatesList.ItemsSource = _templates.OrderBy(t => t.Name).ToList();
            EmptyTemplatesLabel.IsVisible = _templates.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load templates: {ex.Message}", "OK");
            TemplatesList.ItemsSource = new List<EmailTemplate>();
            EmptyTemplatesLabel.IsVisible = true;
        }
    }

    private async void OnAddTemplateClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new EmailTemplateDetailsPage());
    }

    private async void OnTemplateSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not EmailTemplate template) return;
        TemplatesList.SelectedItem = null;
        await Navigation.PushAsync(new EmailTemplateDetailsPage(template));
    }

    private async void OnDeleteTemplateClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var template = button?.BindingContext as EmailTemplate;
        if (template == null) return;

        var confirm = await DisplayAlert(
            "Delete template?",
            $"Remove '{template.Name}' from your email templates?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteTemplateAsync(template.Id);
            await LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete template: {ex.Message}", "OK");
        }
    }
}
