using System.Web;

namespace TubeMailGorilla.Maui.Views;

/// <summary>
/// In-app PayPal checkout. Hosts PayPal's approval page in a WebView so the
/// user never leaves the app. When PayPal redirects back to our return /
/// cancel URL the navigation is intercepted, the order id extracted, and the
/// page closes itself.
/// </summary>
public partial class PayPalApprovalPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _completion = new();
    private readonly string _returnPrefix;
    private readonly string _cancelPrefix;
    private bool _finished;

    /// <summary>Resolves with the approved order id, or null if cancelled/failed.</summary>
    public Task<string?> Completion => _completion.Task;

    public PayPalApprovalPage(string approvalUrl, string returnUrlPrefix, string cancelUrlPrefix)
    {
        InitializeComponent();
        _returnPrefix = returnUrlPrefix;
        _cancelPrefix = cancelUrlPrefix;
        CheckoutWebView.Navigating += OnNavigating;
        CheckoutWebView.Source = new UrlWebViewSource { Url = approvalUrl };
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (_finished) return;

        var url = e.Url ?? string.Empty;

        if (url.StartsWith(_returnPrefix, StringComparison.OrdinalIgnoreCase))
        {
            _finished = true;
            e.Cancel = true;

            // PayPal appends ?subscription_id=<id>&ba_token=<token> to the
            // return URL for recurring subscriptions. Fall back to `token`
            // just in case (one-time Orders flow).
            string? orderId = null;
            try
            {
                var query = new Uri(url).Query;
                var qs = HttpUtility.ParseQueryString(query);
                orderId = qs["subscription_id"] ?? qs["token"];
            }
            catch { /* fall through with null */ }

            _completion.TrySetResult(orderId);
            Navigation.PopAsync();
        }
        else if (url.StartsWith(_cancelPrefix, StringComparison.OrdinalIgnoreCase))
        {
            _finished = true;
            e.Cancel = true;
            _completion.TrySetResult(null);
            Navigation.PopAsync();
        }
    }
}