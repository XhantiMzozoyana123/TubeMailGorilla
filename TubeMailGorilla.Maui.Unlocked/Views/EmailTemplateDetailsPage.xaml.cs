using System.Collections.ObjectModel;
using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

public partial class EmailTemplateDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly EmailTemplate _template;

    /// <summary>
    /// Token chips shown under the subject / body editors - the same set and
    /// behaviour as the Send page composer and the contact page's one-off
    /// email form (the same token engine resolves them at send time).
    /// </summary>
    private readonly ObservableCollection<TokenOption> _subjectTokens = new();
    private readonly ObservableCollection<TokenOption> _bodyTokens = new();

    /// <summary>Which compose field last had focus (for token insertion).</summary>
    private FocusedField _lastFocusedField = FocusedField.Body;

    public EmailTemplateDetailsPage(EmailTemplate? template = null, string? defaultSubject = null, string? defaultBody = null)
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _template = template ?? new EmailTemplate();

        NameEntry.Text = _template.Name;

        if (_template.Id == 0)
        {
            // Storing a new template, optionally pre-filled from the Send page composer.
            SubjectEntry.Text = defaultSubject ?? string.Empty;
            BodyEditor.Text = defaultBody ?? string.Empty;
        }
        else
        {
            SubjectEntry.Text = _template.Subject;
            BodyEditor.Text = _template.Body;
        }

        // Built-in chips as a safe first render; OnAppearing merges in any
        // saved custom parameters (the send loop resolves both the same way).
        AddToken(_subjectTokens, "[name]");
        AddToken(_subjectTokens, "[f_name]");
        AddToken(_subjectTokens, "[l_name]");
        AddToken(_subjectTokens, "[channel]");

        AddToken(_bodyTokens, "[name]");
        AddToken(_bodyTokens, "[f_name]");
        AddToken(_bodyTokens, "[l_name]");
        AddToken(_bodyTokens, "[channel]");
        AddToken(_bodyTokens, "[email]");
        AddToken(_bodyTokens, "[video-title]");
        AddToken(_bodyTokens, "[icebreaker]");
        AddToken(_bodyTokens, "[snapshot_random]");
        AddToken(_bodyTokens, "[snapshot_1]");
        AddToken(_bodyTokens, "[snapshot_ai={Select the frame that most needs editing.}]");

        BindableLayout.SetItemsSource(SubjectTokenBar, _subjectTokens);
        BindableLayout.SetItemsSource(BodyTokenBar, _bodyTokens);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Merge the saved custom parameters into the chips, exactly like the
        // Send page composer does for its campaign.
        await LoadTokenParametersAsync();
    }

    /// <summary>
    /// Merges the saved custom parameters into the token chips, the same way
    /// the Send page and the contact page build their composer chips.
    /// </summary>
    private async Task LoadTokenParametersAsync()
    {
        try
        {
            var parameters = await _db.GetMessageParametersAsync();
            var savedTokens = parameters
                .Select(p => p.Token?.Trim().Trim('[', ']') ?? string.Empty)
                .Where(token => token.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(token => $"[{token}]");

            foreach (var token in savedTokens)
            {
                AddToken(_subjectTokens, token);
                AddToken(_bodyTokens, token);
            }
        }
        catch
        {
            // Built-ins are already showing; a parameter load failure just
            // leaves the custom chips out for this visit.
        }
    }

    private static void AddToken(ObservableCollection<TokenOption> tokens, string token)
    {
        if (tokens.Any(t => t.Token.Equals(token, StringComparison.OrdinalIgnoreCase)))
            return;
        tokens.Add(new TokenOption(token, token));
    }

    private void OnSubjectFocused(object? sender, FocusEventArgs e) => _lastFocusedField = FocusedField.Subject;

    private void OnSubjectUnfocused(object? sender, FocusEventArgs e) { }

    private void OnBodyFocused(object? sender, FocusEventArgs e) => _lastFocusedField = FocusedField.Body;

    private void OnBodyUnfocused(object? sender, FocusEventArgs e) { }

    /// <summary>
    /// Inserts the tapped token into the last-focused field (defaults to the
    /// body editor), mirroring the Send page composer.
    /// </summary>
    private void OnTokenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: string token } || string.IsNullOrWhiteSpace(token))
            return;

        if (_lastFocusedField == FocusedField.Subject)
        {
            SubjectEntry.Text = (SubjectEntry.Text ?? string.Empty) + token + " ";
            SubjectEntry.Focus();
        }
        else
        {
            var body = BodyEditor.Text ?? string.Empty;
            var cursor = Math.Clamp(BodyEditor.CursorPosition, 0, body.Length);
            BodyEditor.Text = body.Insert(cursor, token + " ");
            BodyEditor.Focus();
        }
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        _template.Name = (NameEntry.Text ?? string.Empty).Trim();
        _template.Subject = SubjectEntry.Text ?? string.Empty;
        _template.Body = BodyEditor.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_template.Name))
        {
            await DisplayAlert("Name required", "Please give this template a name.", "OK");
            return;
        }

        try
        {
            _template.UpdatedAt = DateTime.Now;
            await _db.SaveTemplateAsync(_template);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not save template: {ex.Message}", "OK");
        }
    }

    private async void OnCancel(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
