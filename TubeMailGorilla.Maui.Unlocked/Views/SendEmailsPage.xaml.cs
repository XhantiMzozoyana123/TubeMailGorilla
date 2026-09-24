using System.Collections.ObjectModel;
using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

public partial class SendEmailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly EmailService _email;
    private readonly PaymentService _payments;
    private readonly ValidationService _validator;
    private EntitlementInfo _entitlements = new();
    private bool _isSending;

    // Sending options (moved here from Settings)
    private List<Sender> _activeAccounts = new();
    private bool _pickerInitializing;

    // Message rotation variations (bound to the variations list in XAML)
    public ObservableCollection<MessageVariation> Variations { get; } = new();
    private List<MessageVariation> _variations = new();
    private int _editingVariationIndex = -1; // -1 = adding a new one

    // Token insert chips (bound to the subject / body token bars in XAML)
    public ObservableCollection<TokenOption> SubjectTokens { get; } = new();
    public ObservableCollection<TokenOption> BodyTokens { get; } = new();
    private FocusedField _focusedField = FocusedField.Body;

    /// <summary>Last field that had focus (for tappable token insertion).</summary>
    private View? _lastFocusedField;

    /// <summary>Cached campaign stats for the review summary.</summary>
    private int _leadCount;
    private int _activeSenderCount;
    private string _defaultSenderLabel = string.Empty;

    public SendEmailsPage()
    {
        BindingContext = this;
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _email = ServiceHelper.GetService<EmailService>();
        _payments = ServiceHelper.GetService<PaymentService>();
        _validator = ServiceHelper.GetService<ValidationService>();

        // Composer chips are populated from the saved parameters in OnAppearing.
        // Keep the built-ins as a safe fallback for the first render.
        AddTokenOption(SubjectTokens, "[name]", "[name]");
        AddTokenOption(SubjectTokens, "[channel]", "[channel]");
        AddTokenOption(BodyTokens, "[name]", "[name]");
        AddTokenOption(BodyTokens, "[channel]", "[channel]");
        AddTokenOption(BodyTokens, "[email]", "[email]");
        AddTokenOption(BodyTokens, "[icebreaker]", "[icebreaker]");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Server-authoritative plan limits (fail closed to FREE limits).
        _entitlements = await _payments.GetEntitlementsAsync();
        await LoadStatsAsync();
        LoadTemplate();
        await LoadTemplatesIntoPickerAsync();
        await LoadTokenOptionsAsync();
        await LoadSendingOptionsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            var contacts = await _db.GetContactsAsync();
            var senders = await _db.GetSendersAsync();

            UpdateSummary();

            // Note: RemotePlanLimits describes the current plan (limits messaging only).
        }
        catch (Exception ex)
        {
            RecipientsValueLabel.Text = "0";
            await DisplayAlert("Error", $"Could not load stats: {ex.Message}", "OK");
        }
    }

    private async Task LoadTokenOptionsAsync()
    {
        try
        {
            var parameters = await _db.GetMessageParametersAsync();
            var savedTokens = parameters
                .Select(p => p.Token?.Trim().Trim('[', ']') ?? string.Empty)
                .Where(token => token.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(token => $"[{token}]")
                .ToList();

            var subjectTokens = savedTokens
                .Concat(new[] { "[name]", "[channel]" })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var bodyTokens = savedTokens
                .Concat(new[] { "[name]", "[email]", "[channel]", "[video-title]", "[icebreaker]" })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            SubjectTokens.Clear();
            foreach (var token in subjectTokens)
                AddTokenOption(SubjectTokens, token, token);
            BodyTokens.Clear();
            foreach (var token in bodyTokens)
                AddTokenOption(BodyTokens, token, token);
        }
        catch (Exception ex)
        {
            TemplateStatusLabel.Text = $"Could not load email parameters: {ex.Message}";
        }
    }

    private static void AddTokenOption(ObservableCollection<TokenOption> options, string label, string token)
    {
        if (options.Any(option => option.Token.Equals(token, StringComparison.OrdinalIgnoreCase)))
            return;
        options.Add(new TokenOption(label, token));
    }

    private void LoadTemplate()
    {
        SubjectEntry.Text = Preferences.Default.Get("EmailSubject", string.Empty);
        BodyEditor.Text = Preferences.Default.Get("EmailBody", string.Empty);
    }

    // ------------------------------------------------------------------
    //  SENDING OPTIONS (moved here from Settings so everything about a
    //  campaign lives in one place).
    // ------------------------------------------------------------------

    private async Task LoadSendingOptionsAsync()
    {
        try
        {
            var accounts = await _db.GetAllSendersAsync();
            _activeAccounts = accounts.Where(a => a.IsActive).ToList();

            _pickerInitializing = true;
            DefaultAccountPicker.Items.Clear();
            DefaultAccountPicker.Items.Add("(first active account)");
            foreach (var s in _activeAccounts)
                DefaultAccountPicker.Items.Add($"{s.Name} — {s.EmailAddress}");

            var defaultId = SendSettings.DefaultSenderId;
            var selectedIndex = 0;
            if (defaultId > 0)
            {
                var pos = _activeAccounts.FindIndex(a => a.Id == defaultId);
                if (pos >= 0) selectedIndex = pos + 1; // +1 for "(first active account)"
            }
            DefaultAccountPicker.SelectedIndex = selectedIndex;
            _pickerInitializing = false;

            AccountRotationSwitch.IsToggled = SendSettings.AllowAccountRotation;
            MessageRotationSwitch.IsToggled = SendSettings.AllowMessageRotation;
            DefaultAccountPicker.IsEnabled = !SendSettings.AllowAccountRotation;
        }
        catch
        {
            // Non-fatal: the send flow re-checks accounts before sending.
        }

        RefreshVariationsUi();
    }

    private void OnAccountRotationToggled(object? sender, ToggledEventArgs e)
    {
        SendSettings.AllowAccountRotation = e.Value;
        DefaultAccountPicker.IsEnabled = !e.Value;
    }

    private void OnMessageRotationToggled(object? sender, ToggledEventArgs e)
    {
        SendSettings.AllowMessageRotation = e.Value;
        VariationsSection.IsVisible = e.Value;
        CloseVariationEditor();
        UpdateReviewSummary();
    }

    /// <summary>
    /// Inserts a token (e.g. [name]) into the last-focused field
    /// (Subject or Message). Defaults to the Message editor.
    /// </summary>
    private void OnTokenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { Text: string token } || string.IsNullOrWhiteSpace(token))
            return;

        if (ReferenceEquals(_lastFocusedField, SubjectEntry))
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

    private void OnSubjectFocused(object? sender, FocusEventArgs e) => _lastFocusedField = SubjectEntry;

    private void OnSubjectUnfocused(object? sender, FocusEventArgs e) { }

    private void OnBodyFocused(object? sender, FocusEventArgs e) => _lastFocusedField = BodyEditor;

    private void OnBodyUnfocused(object? sender, FocusEventArgs e) { }

    private void UpdateSummary()
    {
        _leadCount = 0;
        _activeSenderCount = 0;
        _defaultSenderLabel = string.Empty;
        UpdateReviewSummary();
    }

    private void UpdateReviewSummary()
    {
        if (SendSummaryLabel != null)
            SendSummaryLabel.Text = "Review your message, then send.";
    }

    // ------------------------------------------------------------------
    //  MESSAGE VARIATIONS — the actual messages used when rotation is on.
    // ------------------------------------------------------------------

    private void RefreshVariationsUi()
    {
        _variations = SendSettings.MessageVariations;

        Variations.Clear();
        foreach (var v in _variations)
            Variations.Add(v);

        var rotationOn = MessageRotationSwitch?.IsToggled ?? false;
        VariationsSection.IsVisible = rotationOn;

        // The main subject/message form is unused while variations drive
        // the campaign, so hide it to keep the page focused.
        ComposeSection.IsVisible = !rotationOn;

        EmptyVariationsLabel.IsVisible = _variations.Count == 0;
        AddVariationButton.Text = _variations.Count == 0 ? "Add First Variation" : "Add Another Variation";
    }

    private void OnAddVariationClicked(object? sender, EventArgs e)
    {
        _editingVariationIndex = -1;
        VarSubjectEntry.Text = string.Empty;
        VarBodyEditor.Text = string.Empty;
        VariationEditor.IsVisible = true;
        VarSubjectEntry.Focus();
    }

    private void OnCancelVariationClicked(object? sender, EventArgs e)
        => CloseVariationEditor();

    private void CloseVariationEditor()
    {
        VariationEditor.IsVisible = false;
        _editingVariationIndex = -1;
    }

    private void OnSaveVariationClicked(object? sender, EventArgs e)
    {
        var subject = VarSubjectEntry.Text?.Trim() ?? string.Empty;
        var body = VarBodyEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            DisplayAlert("Almost there", "Give the variation both a subject and a message.", "Got it");
            return;
        }

        if (_editingVariationIndex >= 0 && _editingVariationIndex < _variations.Count)
            _variations[_editingVariationIndex] = new MessageVariation(subject, body);
        else
            _variations.Add(new MessageVariation(subject, body));

        SendSettings.MessageVariations = _variations;
        CloseVariationEditor();
        RefreshVariationsUi();
        UpdateReviewSummary();
    }

    private void OnRemoveVariationClicked(object? sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is not MessageVariation variation) return;

        var index = _variations.IndexOf(variation);
        if (index < 0) return;

        _variations.RemoveAt(index);
        SendSettings.MessageVariations = _variations;
        RefreshVariationsUi();
        UpdateReviewSummary();
    }

    private void OnDefaultAccountSelected(object? sender, EventArgs e)
    {
        if (_pickerInitializing) return;

        if (DefaultAccountPicker.SelectedIndex < 1 ||
            DefaultAccountPicker.SelectedIndex - 1 >= _activeAccounts.Count)
        {
            SendSettings.DefaultSenderId = 0; // "(first active account)"
            return;
        }

        SendSettings.DefaultSenderId = _activeAccounts[DefaultAccountPicker.SelectedIndex - 1].Id;
    }

    // ------------------------------------------------------------------
    //  TEMPLATE PICKER — loads a saved template INTO this form so the
    //  user can tweak it and send. Templates themselves are managed on
    //  the Email Templates page, not here.
    // ------------------------------------------------------------------

    private List<EmailTemplate> _templates = new();

    private async Task LoadTemplatesIntoPickerAsync()
    {
        try
        {
            _templates = await _db.GetTemplatesAsync();

            TemplatePicker.Items.Clear();
            TemplatePicker.Items.Add("No template - write from scratch");
            foreach (var t in _templates)
                TemplatePicker.Items.Add(t.Name);

            TemplatePicker.SelectedIndex = 0;
            TemplateStatusLabel.Text = _templates.Count == 0
                ? "You have no saved templates yet. Create them on the Email Templates page."
                : $"{_templates.Count} template{(_templates.Count == 1 ? "" : "s")} available.";
        }
        catch (Exception ex)
        {
            TemplateStatusLabel.Text = $"Could not load templates: {ex.Message}";
        }
    }

    private async void OnTemplateSelected(object? sender, EventArgs e)
    {
        var index = TemplatePicker.SelectedIndex - 1; // index 0 = "no template"
        if (index < 0 || index >= _templates.Count)
        {
            TemplateStatusLabel.Text = string.Empty;
            return;
        }

        var template = _templates[index];

        // Templates are a gated feature - ask the API before loading one.
        if (!await EnsureTemplatesAllowedAsync("using email templates"))
        {
            TemplatePicker.SelectedIndex = 0;
            return;
        }

        if (!string.IsNullOrWhiteSpace(template.Subject))
            SubjectEntry.Text = template.Subject;
        if (!string.IsNullOrWhiteSpace(template.Body))
            BodyEditor.Text = template.Body;

        TemplateStatusLabel.Text = $"Loaded \"{template.Name}\" into the form. Tweak it if you like, then hit Send Campaign.";
    }

    /// <summary>
    /// Email templates are Pro-only. The API's gatekeeper decides - this is
    /// just transport for its verdict plus the upgrade path.
    /// </summary>
    private async Task<bool> EnsureTemplatesAllowedAsync(string action)
    {
        var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.UseEmailTemplates);
        if (verdict.Approved) return true;

        var openSubscription = await DisplayAlert(
            "Pro feature",
            $"{verdict.Reason}\n\nUpgrade now?",
            "Upgrade", "Not now");

        if (openSubscription)
        {
            try
            {
                await Browser.Default.OpenAsync(_payments.GetUpgradeWebsiteUrl(), BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        return false;
    }


    private async void OnSendEmailsClicked(object? sender, EventArgs e)
    {
        if (_isSending) return;

        SendProgressBar.Progress = 0;
        SendProgressBar.IsVisible = true;
        SendingIndicator.IsVisible = true;
        SendingIndicator.IsRunning = true;
        SendButton.IsEnabled = false;
        StatusLabel.Text = "Preparing your campaign…";

        var subject = SubjectEntry.Text?.Trim() ?? string.Empty;
        var body = BodyEditor.Text?.Trim() ?? string.Empty;

        // When message rotation is on with valid variations, the hidden
        // compose form is not used — variations are the campaign instead.
        var hasValidVariations = SendSettings.MessageVariations.Any(v =>
            !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body));
        var usingRotation = SendSettings.AllowMessageRotation && hasValidVariations;

        if (!usingRotation && (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body)))
        {
            StatusLabel.Text = "Please write both a subject and an email body first (or pick a template above).";
            SendProgressBar.IsVisible = false;
            SendingIndicator.IsVisible = false;
            SendingIndicator.IsRunning = false;
            SendButton.IsEnabled = true;
            return;
        }

        // Save current as template
        Preferences.Default.Set("EmailSubject", subject);
        Preferences.Default.Set("EmailBody", body);

        try
        {
            var allContacts = await _db.GetContactsAsync();
            var senders = await _db.GetSendersAsync();

            // GATEKEEPER: the API must approve this campaign BEFORE any email is sent.
            var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.SendEmails, allContacts.Count);
            if (!verdict.Approved)
            {
                SendProgressBar.IsVisible = false;
                SendingIndicator.IsRunning = false;
                return;
            }

            // Respect the server-approved limit (never trust local settings).
            var cappedByPlan = verdict.Limit >= 0 && allContacts.Count > verdict.Limit;
            var contacts = cappedByPlan ? allContacts.Take(verdict.Limit).ToList() : allContacts;

            if (cappedByPlan)
            {
                await DisplayAlert("Plan limit",
                    $"Your plan allows campaigns of up to {verdict.Limit} emails - this campaign will target the first {verdict.Limit} of your {allContacts.Count} leads.",
                    "OK");
            }

            if (senders.Count == 0)
            {
                StatusLabel.Text = "No active email accounts. Add one in Settings → Email Accounts.";
                SendProgressBar.IsVisible = false;
                SendingIndicator.IsRunning = false;
                await DisplayAlert("No Accounts", "Add an SMTP account in Settings first.", "OK");
                return;
            }

            if (contacts.Count == 0)
            {
                StatusLabel.Text = "No contacts to email. Extract leads from the Extract page first.";
                SendProgressBar.IsVisible = false;
                SendingIndicator.IsRunning = false;
                return;
            }

            _isSending = true;
            var parameters = await _db.GetMessageParametersAsync();
            var sent = 0;
            var failed = 0;
            var skipped = 0;

            // Message rotation: if enabled and variations exist, each email
            // takes its turn across the recipients.
            _variations = SendSettings.MessageVariations.Where(v =>
                !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body)).ToList();
            var rotateMessages = usingRotation && _variations.Count > 0;

            // Load each lead's latest AI icebreaker (Opener) once so [icebreaker]
            // tokens can be personalized per recipient.
            var openers = await _db.GetOpenersAsync();
            var latestOpenerByContact = openers
                .Where(o => o.EmailerId > 0 && !string.IsNullOrWhiteSpace(o.Text))
                .GroupBy(o => o.EmailerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First().Text.Trim());

            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                StatusLabel.Text = $"Sending {i + 1}/{contacts.Count}… ({contact.Email})";

                // A lead without an icebreaker is skipped - every email must
                // open with its personalized first line.
                if (!latestOpenerByContact.TryGetValue(contact.Id, out var icebreaker))
                {
                    skipped++;
                    SendProgressBar.Progress = (double)(i + 1) / contacts.Count;
                    continue;
                }

                // Select sender (rotate if enabled; otherwise use the configured default account)
                Sender? senderAccount;
                if (SendSettings.AllowAccountRotation && senders.Count > 1)
                {
                    senderAccount = senders[i % senders.Count];
                }
                else
                {
                    // Prefer the configured default account when rotation is off.
                    var defaultId = SendSettings.DefaultSenderId;
                    senderAccount = senders[0];
                    if (defaultId > 0)
                    {
                        foreach (var s in senders)
                        {
                            if (s.Id == defaultId)
                            {
                                senderAccount = s;
                                break;
                            }
                        }
                    }
                }

                // Pick this email's message: rotated variation or the main one.
                string messageSubject = subject;
                string messageBody = body;
                if (rotateMessages)
                {
                    var variation = _variations[i % _variations.Count];
                    messageSubject = variation.Subject;
                    messageBody = variation.Body;
                }

                var personalizedSubject = EmailService.Personalize(messageSubject, contact, parameters, icebreaker);
                var personalizedBody = EmailService.ToHtmlBody(
                    EmailService.Personalize(messageBody, contact, parameters, icebreaker));

                var message = new MessengerDto
                {
                    EmailFrom = senderAccount.EmailAddress,
                    FromName = senderAccount.Name,
                    EmailTo = contact.Email,
                    ToName = contact.Name ?? contact.Email,
                    Subject = personalizedSubject,
                    Body = personalizedBody,
                    SmtpHost = senderAccount.SmtpHost,
                    SmtpPort = senderAccount.SmtpPort,
                    SmtpUser = senderAccount.SmtpUser,
                    SmtpPassword = senderAccount.SmtpPassword
                };

                var success = await _email.SendEmailAsync(message);
                if (success)
                    sent++;
                else
                    failed++;

                // Small delay to avoid hitting rate limits
                await Task.Delay(500);

                SendProgressBar.Progress = (double)(i + 1) / contacts.Count;
            }

            StatusLabel.Text = $"All done. Sent: {sent}, Failed: {failed}, Skipped (no icebreaker/blocked): {skipped}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Send failed: {ex.Message}";
        }
        finally
        {
            _isSending = false;
            SendProgressBar.IsVisible = false;
            SendingIndicator.IsVisible = false;
            SendingIndicator.IsRunning = false;
            SendButton.IsEnabled = true;
            await LoadStatsAsync();
        }
    }
}