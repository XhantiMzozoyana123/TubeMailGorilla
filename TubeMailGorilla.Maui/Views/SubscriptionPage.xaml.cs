using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

/// <summary>
/// Shows the current subscription state (fetched from the API - the same
/// state the website displays). Upgrading redirects to the website's pricing
/// page so there is a single payment experience across platforms.
/// </summary>
public partial class SubscriptionPage : ContentPage
{
    private readonly PaymentService _payments;
    private bool _isSubscribed;
    private decimal _price = 9.99m;
    private string _currency = "USD";
    private string _planName = "Pro";
    private DateTime? _nextBilling;

    public SubscriptionPage()
    {
        InitializeComponent();
        _payments = ServiceHelper.GetService<PaymentService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshStateAsync();
    }

    private async Task RefreshStateAsync()
    {
        SetBusy(true);
        try
        {
            // One authoritative source: the API's subscription status endpoint.
            var status = await _payments.GetSubscriptionStatusAsync();
            if (status is not null)
            {
                _isSubscribed = status.IsSubscribed;
                _price = status.Price;
                _currency = status.Currency;
                _planName = string.IsNullOrWhiteSpace(status.PlanName) ? "Pro" : status.PlanName;
                _nextBilling = status.NextBillingDate;
            }
            else
            {
                // Offline fallback: last known price + local check.
                var pricing = await _payments.GetPricingAsync();
                if (pricing is not null)
                {
                    _price = pricing.Amount;
                    _currency = pricing.Currency;
                }
                _isSubscribed = await _payments.IsSubscribedAsync();
            }

            UpdateUi();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateUi()
    {
        StatusLabel.Text = _isSubscribed ? $"{_planName} plan" : "Free plan";
        StatusLabel.TextColor = _isSubscribed
            ? Color.FromArgb("#2E9E5B")
            : Application.Current!.Resources.TryGetValue("TextPrimary", out var v) && v is Color c ? c : Colors.Gray;

        PriceLabel.Text = _isSubscribed
            ? $"{_planName} - {_price:0.00} {_currency} / month"
            : $"Upgrade to {_planName} - {_price:0.00} {_currency} / month";

        if (_isSubscribed)
        {
            BenefitsLabel.Text = _nextBilling is not null
                ? $"Next payment on {_nextBilling.Value.ToLocalTime():d}. Works on the desktop app and the website."
                                : "Works on the desktop app and the website. Thank you for being a member!";
        }
        else
        {
            BenefitsLabel.Text = "Upgrading takes you to our website - your Pro subscription then works on both the website and this app.";
        }

        UpgradeButton.IsVisible = !_isSubscribed;
        CancelButton.IsVisible = _isSubscribed;
    }

    private async void OnUpgradeClicked(object? sender, EventArgs e)
    {
        UpgradeButton.IsEnabled = false;
        SetBusy(true);
        try
        {
            // Single payment experience: upgrade on the website. The website
            // handles PayPal checkout; once subscribed, this page reflects it
            // automatically next time it loads.
            await Browser.Default.OpenAsync(_payments.GetUpgradeWebsiteUrl(), BrowserLaunchMode.SystemPreferred);
            await DisplayAlert("Upgrade on our website", "Complete your upgrade in the browser that just opened. Your Pro benefits appear here automatically afterwards.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            UpgradeButton.IsEnabled = true;
            SetBusy(false);
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Cancel subscription",
            "You will lose access to premium features. Continue?",
            "Cancel subscription", "Keep it");

        if (!confirm) return;

        CancelButton.IsEnabled = false;
        SetBusy(true);
        try
        {
            var (success, message) = await _payments.CancelSubscriptionAsync();
            await DisplayAlert(success ? "Subscription cancelled" : "Error",
                message ?? "Done.", "OK");
            await RefreshStateAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            CancelButton.IsEnabled = true;
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
    }
}
