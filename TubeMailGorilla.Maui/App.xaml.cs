namespace TubeMailGorilla.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Check if user is already logged in
        var hasToken = Preferences.Get("AuthToken", string.Empty) is { Length: > 0 };

        // Show auth page for unauthenticated users
        Page startPage = hasToken
            ? new AppShell()
            : new NavigationPage(new Views.AuthView());

        var window = new Window(startPage)
        {
            Title = "TubeMailGorilla"
        };

        // A stored token does NOT guarantee it is still valid (expired,
        // revoked, or from an older database). Validate it against the API;
        // if rejected, clear it and fall back to the login screen.
        if (hasToken)
        {
            _ = ValidateStoredTokenAsync();
        }

        return window;
    }

    /// <summary>
    /// Replaces the visible window with a brand-new one rooted at
    /// <paramref name="root"/>. Opening a fresh window (instead of mutating
    /// <c>Window.Page</c>) is used because page swaps on an existing window
    /// do not reliably re-render on Windows.
    /// </summary>
    public static void ReplaceRoot(Page root)
    {
        var app = Application.Current;
        if (app is null) return;

        var newWindow = new Window(root) { Title = "TubeMailGorilla" };

        var oldWindows = app.Windows?.ToList();
        app.OpenWindow(newWindow);

        if (oldWindows is not null)
        {
            foreach (var old in oldWindows)
            {
                app.CloseWindow(old);
            }
        }
    }

    /// <summary>Shows the login/register screen in place of everything else.</summary>
    public static void ShowLoginScreen() => ReplaceRoot(new NavigationPage(new Views.AuthView()));

    private static async Task ValidateStoredTokenAsync()
    {
        try
        {
            var authService = Services.ServiceHelper.GetService<Services.AuthService>();
            var result = await authService.GetCurrentUserAsync();

            if (!result.Success)
            {
                // Token invalid/expired -> forget it and show the login screen.
                Preferences.Remove("AuthToken");
                await MainThread.InvokeOnMainThreadAsync(ShowLoginScreen);
            }
        }
        catch
        {
            // Network / API unavailable - keep the user where they are.
            // They will be prompted to re-authenticate when an API call fails.
        }
    }
}