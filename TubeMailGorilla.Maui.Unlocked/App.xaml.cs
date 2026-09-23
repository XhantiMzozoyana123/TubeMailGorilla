namespace TubeMailGorilla.Maui.Unlocked;

/// <summary>
/// Unlocked edition bootstrap: the app opens straight into the main shell.
/// There is no sign-in, no account and no server-side entitlement check -
/// every feature is available from the first launch.
/// </summary>
public partial class App : Application
{
    /// <summary>Temporary startup tracing hook (remove with crash logging).</summary>
    public static Action<string>? TraceStartup { get; set; }

    public App()
    {
        TraceStartup?.Invoke("App ctor begin");
        InitializeComponent();
        TraceStartup?.Invoke("App ctor complete");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        TraceStartup?.Invoke("CreateWindow begin (unlocked edition - no sign-in required)");

        var startPage = new AppShell();
        TraceStartup?.Invoke("CreateWindow startPage created: AppShell");

        var window = new Window(startPage)
        {
            Title = "TubeMailGorilla Unlocked"
        };
        TraceStartup?.Invoke("CreateWindow window created");
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

        var newWindow = new Window(root) { Title = "TubeMailGorilla Unlocked" };

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

    /// <summary>Returns to the main shell (used by reset-style actions).</summary>
    public static void ShowMainScreen() => ReplaceRoot(new AppShell());
}
