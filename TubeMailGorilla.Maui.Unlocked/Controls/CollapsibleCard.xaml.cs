using System.Windows.Input;

namespace TubeMailGorilla.Maui.Unlocked.Controls;

/// <summary>
/// A styled card ("box") whose body can be minimized to just the header and
/// maximized again by clicking the header. The whole header row is clickable.
/// The card chrome (header + body presenter) is defined by the ControlTemplate;
/// template parts are located by AutomationId.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CollapsibleCard : ContentView
{
    private const string HeaderId = "CardHeader";
    private const string TitleId = "CardTitle";
    private const string ChevronId = "CardChevron";
    private const string DividerId = "CardDivider";

    private bool _gestureAttached;

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CollapsibleCard), string.Empty,
            propertyChanged: OnTitleChanged);

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(CollapsibleCard), null,
            propertyChanged: OnBodyChanged);

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(CollapsibleCard), true,
            propertyChanged: OnIsExpandedChanged);

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(CollapsibleCard));

    /// <summary>Raised after the card is minimized or maximized.</summary>
    public event EventHandler<bool>? ExpandedChanged;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The card body content (the part that minimizes away).</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>True when the body is visible (maximized).</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Optional command invoked on every toggle (bool = new IsExpanded).</summary>
    public ICommand ToggleCommand
    {
        get => (ICommand)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public CollapsibleCard()
    {
        InitializeComponent();
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        // Template chrome (header/title/chevron) is added as children once the
        // ControlTemplate inflates; refresh our references whenever that happens.
        RefreshTemplateParts(animate: false);
    }

    private static void OnTitleChanged(BindableObject d, object oldValue, object newValue) =>
        ((CollapsibleCard)d).RefreshTemplateParts(animate: false);

    private static void OnBodyChanged(BindableObject d, object oldValue, object newValue) =>
        ((CollapsibleCard)d).Content = (View?)newValue;

    private static void OnIsExpandedChanged(BindableObject d, object oldValue, object newValue) =>
        ((CollapsibleCard)d).RefreshTemplateParts(animate: true);

    private void OnHeaderTapped(object? sender, EventArgs e)
    {
        IsExpanded = !IsExpanded;
        ToggleCommand?.Execute(IsExpanded);
    }

    private T? FindPart<T>(string automationId) where T : Element =>
        this.GetVisualTreeDescendants().OfType<T>().FirstOrDefault(e => e.AutomationId == automationId);

    private void RefreshTemplateParts(bool animate)
    {
        if (FindPart<Label>(TitleId) is not { } title) return; // template not applied yet

        title.Text = Title ?? string.Empty;

        if (FindPart<Grid>(HeaderId) is { } header && !_gestureAttached)
        {
            _gestureAttached = true;
            header.GestureRecognizers.Add(new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Command = new Command(() => Toggle()),
            });
        }

        if (FindPart<BoxView>(DividerId) is { } divider)
            divider.IsVisible = IsExpanded;

        var body = Content;
        if (body is null)
        {
            ExpandedChanged?.Invoke(this, IsExpanded);
            return;
        }

        body.IsVisible = IsExpanded;

        if (!animate)
        {
            body.Opacity = IsExpanded ? 1 : 0;
            ExpandedChanged?.Invoke(this, IsExpanded);
            return;
        }

        if (IsExpanded)
        {
            body.IsVisible = true;
            body.Opacity = 0;
            body.FadeTo(1, 180, Easing.CubicOut);
        }
        else
        {
            var fade = body.FadeTo(0, 140, Easing.CubicIn);
            fade.ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() => body.IsVisible = false);
            });
        }

        ExpandedChanged?.Invoke(this, IsExpanded);
    }

    /// <summary>Toggles the card with animation (useful from code-behind).</summary>
    public void Toggle() => IsExpanded = !IsExpanded;
}
