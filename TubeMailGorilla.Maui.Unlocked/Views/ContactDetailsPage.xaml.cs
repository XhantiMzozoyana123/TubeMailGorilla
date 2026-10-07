using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

public partial class ContactDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly YouTubeTranscriptService _transcript;
    private readonly EmailService _email;
    private readonly ValidationService _validator;
    private bool _isGeneratingIcebreaker;
    private bool _isGeneratingImprovements;

    /// <summary>
    /// Sender picker state for the direct-send card, mirroring SendEmailsPage's
    /// LoadSendingOptionsAsync wiring (active accounts only, default taken from
    /// SendSettings.DefaultSenderId).
    /// </summary>
    private List<Sender> _directAccounts = new();
    private bool _directPickerInitializing;

    /// <summary>Token chips shown above the two direct-send editors.</summary>
    private readonly List<TokenOption> _directSubjectTokens = new();
    private readonly List<TokenOption> _directBodyTokens = new();

    /// <summary>True while a direct send/preview is running (blocks double-taps).</summary>
    private bool _isSendingDirect;

    /// <summary>Last direct-send field that had focus, for token insertion.</summary>
    private View? _directLastFocusedField;

    /// <summary>Re-captures this lead's frames on demand (the "Try again" button).</summary>
    private readonly VideoSnapshotService _snapshotCapture;

    /// <summary>True while a manual capture is running, to block double-taps.</summary>
    private bool _isCapturingSnapshots;

    /// <summary>The lead's snapshots, ordered as they were captured.</summary>
    private readonly List<VideoSnapshotItem> _snapshots = new();

    /// <summary>The video these snapshots came from, for the "watch" link.</summary>
    private string? _videoUrl;

    public ContactDetailsPage(EmailContact contact)
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _ai = ServiceHelper.GetService<AIService>();
        _transcript = ServiceHelper.GetService<YouTubeTranscriptService>();
        _snapshotCapture = ServiceHelper.GetService<VideoSnapshotService>();
        _email = ServiceHelper.GetService<EmailService>();
        _validator = ServiceHelper.GetService<ValidationService>();
        BindingContext = contact;
        NameEntry.Text = contact.Name ?? string.Empty;
        EmailEntry.Text = contact.Email;
        LoadSnapshots(contact);
        SetupDirectSend(contact);
        _ = LoadOpenersAsync();
    }

    /// <summary>
    /// Prepares the direct-send card: prefill the recipient from the contact,
    /// seed the token chips with the built-ins (saved parameters are merged in
    /// OnAppearing), and load the sender accounts.
    /// </summary>
    private void SetupDirectSend(EmailContact contact)
    {
        DirectToEntry.Text = contact.Email;

        // Built-in chips as a safe first render; OnAppearing merges in any
        // saved custom parameters and refreshes the snapshot hint.
        AddDirectToken(_directSubjectTokens, "[name]");
        AddDirectToken(_directSubjectTokens, "[f_name]");
        AddDirectToken(_directSubjectTokens, "[l_name]");
        AddDirectToken(_directSubjectTokens, "[channel]");
        AddDirectToken(_directBodyTokens, "[name]");
        AddDirectToken(_directBodyTokens, "[f_name]");
        AddDirectToken(_directBodyTokens, "[l_name]");
        AddDirectToken(_directBodyTokens, "[email]");
        AddDirectToken(_directBodyTokens, "[channel]");
        AddDirectToken(_directBodyTokens, "[video-title]");
        AddDirectToken(_directBodyTokens, "[icebreaker]");
        AddDirectToken(_directBodyTokens, "[snapshot_random]");
        AddDirectToken(_directBodyTokens, "[snapshot_1]");
        AddDirectToken(_directBodyTokens, "[snapshot_ai={Select the frame that most needs editing.}]");

        BindableLayout.SetItemsSource(DirectSubjectTokenBar, _directSubjectTokens);
        BindableLayout.SetItemsSource(DirectBodyTokenBar, _directBodyTokens);

        DirectSendStatusLabel.Text = contact.HasVideoSnapshots
            ? $"This lead has {contact.VideoSnapshot.Count} frame{(contact.VideoSnapshot.Count == 1 ? "" : "s")} - [snapshot_1]…[snapshot_{contact.VideoSnapshot.Count}] all work."
            : "This lead has no frames yet, so snapshot tokens will render empty.";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDirectAccountsAsync();
        await LoadDirectTokenParametersAsync();
    }

    // ------------------------------------------------------------------
    //  Video snapshots
    // ------------------------------------------------------------------

    /// <summary>
    /// Rebuilds the snapshot carousel from the lead's stored base64 images.
    /// Timestamps are index-aligned with the images but were added later, so
    /// they are read defensively - a lead stored without them still shows.
    /// </summary>
    private void LoadSnapshots(EmailContact contact)
    {
        _snapshots.Clear();
        _videoUrl = contact.VideoUrl;

        var images = contact.VideoSnapshot;
        var timestamps = contact.VideoSnapshotTimestamps;

        for (var i = 0; i < images.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(images[i])) continue;

            var seconds = i < timestamps.Count ? timestamps[i] : 0d;

            _snapshots.Add(new VideoSnapshotItem(images[i], TranscriptCue.FormatTimestamp(seconds), seconds));
        }

        var hasSnapshots = _snapshots.Count > 0;

        // Diagnostic: proves whether the page received images at all, which is
        // the first thing to check when the carousel looks empty.
        Log($"ContactDetails: {contact.Email} images={images.Count} timestamps={timestamps.Count} shown={_snapshots.Count}");

        SnapshotsPanel.IsVisible = hasSnapshots;
        SnapshotsHintLabel.IsVisible = hasSnapshots;
        NoSnapshotsLabel.IsVisible = !hasSnapshots;

        // The watch and video-review buttons need a video URL, and that is
        // independent of whether any snapshots exist, so it is decided here
        // rather than in UpdateSnapshotIndicator (which is skipped when there
        // are none). The ZIP button needs actual frames, not just a URL.
        var hasVideo = !string.IsNullOrWhiteSpace(_videoUrl);
        SnapshotWatchButton.IsVisible = hasSnapshots && hasVideo;
        ActionImprovementsButton.IsVisible = hasVideo;
        ActionDownloadButton.IsEnabled = hasSnapshots;
        ActionRowHintLabel.Text = hasSnapshots
            ? $"{_snapshots.Count} snapshot{(_snapshots.Count == 1 ? "" : "s")} captured - Save ZIP exports them all."
            : "No snapshots captured for this lead yet.";

        // The retry button is only useful when there is a video to capture
        // from; without a URL the row still shows, but explains why it is
        // unavailable instead of offering a button that cannot work.
        RetrySnapshotsButton.IsVisible = hasVideo;
        SnapshotRetryStatusLabel.Text = hasVideo
            ? (hasSnapshots
                ? "Missing frames? Retry the capture for this lead's video."
                : "No frames captured yet. Try again captures them straight from the video.")
            : "This lead has no video URL, so snapshots cannot be captured.";

        if (!hasSnapshots) return;

        // Reset first: the carousel holds a reference to this same list, so
        // assigning it again after a re-capture would not repaint the view.
        SnapshotsCarousel.ItemsSource = null;
        SnapshotsCarousel.ItemsSource = _snapshots;
        SnapshotsCarousel.Position = 0;
        UpdateSnapshotIndicator(0);
    }

    /// <summary>
    /// Re-runs snapshot capture for this contact's video and stores the result.
    ///
    /// Extraction captures frames as it goes and never retries them, so a lead
    /// can end up with none at all - the download timed out, the decoder
    /// failed, or the contact predates snapshot capture. This gives that lead
    /// a second chance without re-running a whole keyword extraction.
    /// </summary>
    private async void OnRetrySnapshotsClicked(object? sender, EventArgs e)
    {
        if (_isCapturingSnapshots) return;
        if (BindingContext is not EmailContact contact) return;

        var videoUrl = contact.VideoUrl?.Trim();
        if (string.IsNullOrWhiteSpace(videoUrl))
        {
            await DisplayAlert("No video",
                "This lead has no video URL, so snapshots cannot be captured.", "OK");
            return;
        }

        _isCapturingSnapshots = true;
        RetrySnapshotsButton.IsEnabled = false;
        RetrySnapshotsButton.Text = "Capturing…";
        SnapshotCaptureIndicator.IsRunning = true;
        SnapshotCaptureIndicator.IsVisible = true;
        SnapshotRetryStatusLabel.Text =
            "Downloading the video and capturing frames - this can take a minute or two…";

        try
        {
            // CaptureAsync never throws: an undownloadable or undecodable
            // video simply yields zero frames, which is handled below.
            var captured = await _snapshotCapture.CaptureAsync(videoUrl);

            if (captured.Count == 0)
            {
                SnapshotRetryStatusLabel.Text =
                    "No snapshots captured. Check your internet connection and try again.";
                await DisplayAlert("No snapshots",
                    "The video could not be downloaded or decoded, so no frames were captured. Check your internet connection and try again.",
                    "OK");
                return;
            }

            contact.VideoSnapshot = captured.Select(s => s.Base64Image).ToList();
            contact.VideoSnapshotTimestamps = captured.Select(s => s.Seconds).ToList();

            // Persist immediately: leaving the page must not lose the frames.
            if (contact.Id == 0)
                await _db.AddContactAsync(contact);
            else
                await _db.UpdateContactAsync(contact);

            LoadSnapshots(contact);
            SnapshotRetryStatusLabel.Text =
                $"{captured.Count} snapshot{(captured.Count == 1 ? "" : "s")} captured and saved.";
            Log($"ContactDetails: retry captured {captured.Count} snapshots for {contact.Email}");
        }
        catch (Exception ex)
        {
            SnapshotRetryStatusLabel.Text = $"Snapshot capture failed: {ex.Message}";
            Log($"ContactDetails: retry failed for {contact.Email}: {ex.Message}");
        }
        finally
        {
            _isCapturingSnapshots = false;
            SnapshotCaptureIndicator.IsRunning = false;
            SnapshotCaptureIndicator.IsVisible = false;
            RetrySnapshotsButton.IsEnabled = true;
            RetrySnapshotsButton.Text = "Try again";
        }
    }

    // ------------------------------------------------------------------
    //  Direct (one-off) email
    // ------------------------------------------------------------------

    /// <summary>
    /// Loads the active sender accounts into the picker, defaulting to
    /// SendSettings.DefaultSenderId - the same wiring SendEmailsPage uses, so
    /// both pages agree on which account "the default" is.
    /// </summary>
    private async Task LoadDirectAccountsAsync()
    {
        try
        {
            var accounts = await _db.GetAllSendersAsync();
            _directAccounts = accounts.Where(a => a.IsActive).ToList();

            _directPickerInitializing = true;
            DirectAccountPicker.Items.Clear();
            foreach (var s in _directAccounts)
                DirectAccountPicker.Items.Add($"{s.Name} — {s.EmailAddress}");

            var defaultId = SendSettings.DefaultSenderId;
            var selectedIndex = 0;
            if (defaultId > 0)
            {
                var pos = _directAccounts.FindIndex(a => a.Id == defaultId);
                if (pos >= 0) selectedIndex = pos;
            }

            DirectAccountPicker.SelectedIndex = _directAccounts.Count == 0 ? -1 : selectedIndex;
            _directPickerInitializing = false;
        }
        catch
        {
            // Non-fatal: the send flow re-checks accounts before sending.
            _directPickerInitializing = false;
        }
    }

    /// <summary>
    /// Merges the saved custom parameters into the token chips, the same way
    /// SendEmailsPage builds its composer chips.
    /// </summary>
    private async Task LoadDirectTokenParametersAsync()
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
                AddDirectToken(_directSubjectTokens, token);
                AddDirectToken(_directBodyTokens, token);
            }
        }
        catch
        {
            // Built-ins are already showing; a parameter load failure just
            // leaves the custom chips out for this visit.
        }
    }

    private void AddDirectToken(List<TokenOption> tokens, string token)
    {
        if (tokens.Any(t => t.Token.Equals(token, StringComparison.OrdinalIgnoreCase)))
            return;
        tokens.Add(new TokenOption(token, token));
    }

    private void OnDirectAccountSelected(object? sender, EventArgs e)
    {
        // Same rule as SendEmailsPage: the picker writes the shared default,
        // so both pages stay in sync.
        if (_directPickerInitializing) return;

        if (DirectAccountPicker.SelectedIndex < 0 ||
            DirectAccountPicker.SelectedIndex >= _directAccounts.Count)
            return;

        SendSettings.DefaultSenderId = _directAccounts[DirectAccountPicker.SelectedIndex].Id;
    }

    private void OnDirectSubjectFocused(object? sender, FocusEventArgs e) => _directLastFocusedField = DirectSubjectEntry;

    private void OnDirectSubjectUnfocused(object? sender, FocusEventArgs e) { }

    private void OnDirectBodyFocused(object? sender, FocusEventArgs e) => _directLastFocusedField = DirectBodyEditor;

    private void OnDirectBodyUnfocused(object? sender, FocusEventArgs e) { }

    /// <summary>
    /// Inserts the tapped token into the last-focused direct-send field
    /// (defaults to the message editor), mirroring SendEmailsPage.
    /// </summary>
    private void OnDirectTokenClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: string token } || string.IsNullOrWhiteSpace(token))
            return;

        if (ReferenceEquals(_directLastFocusedField, DirectSubjectEntry))
        {
            DirectSubjectEntry.Text = (DirectSubjectEntry.Text ?? string.Empty) + token + " ";
            DirectSubjectEntry.Focus();
        }
        else
        {
            var body = DirectBodyEditor.Text ?? string.Empty;
            var cursor = Math.Clamp(DirectBodyEditor.CursorPosition, 0, body.Length);
            DirectBodyEditor.Text = body.Insert(cursor, token + " ");
            DirectBodyEditor.Focus();
        }
    }

    private void OnTestEmailToggled(object? sender, ToggledEventArgs e)
    {
        TestEmailEntry.IsVisible = e.Value;
        if (e.Value)
        {
            DirectSendStatusLabel.Text = "Test mode: the email goes to the address below, not to this lead.";
            TestEmailEntry.Focus();
        }
        else
        {
            DirectSendStatusLabel.Text = $"Sending to {DirectToEntry.Text} (this lead).";
        }
    }

    /// <summary>
    /// Resolves every token in the current subject/message for this lead and
    /// shows the result in an alert. Shares the exact pipeline with the real
    /// send (via <see cref="BuildDirectEmailAsync"/>), so what is previewed is
    /// what will be sent - including which snapshot frame lands in the body.
    /// </summary>
    private async void OnDirectPreviewClicked(object? sender, EventArgs e)
    {
        if (_isSendingDirect) return;
        if (BindingContext is not EmailContact contact) return;

        var subject = DirectSubjectEntry.Text?.Trim() ?? string.Empty;
        var body = DirectBodyEditor.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            DirectSendStatusLabel.Text = "Write both a subject and a message first.";
            return;
        }

        _isSendingDirect = true;
        DirectPreviewButton.IsEnabled = false;
        DirectSendIndicator.IsRunning = true;
        DirectSendIndicator.IsVisible = true;
        DirectSendStatusLabel.Text = "Resolving tokens for this lead…";

        try
        {
            var message = await BuildDirectEmailAsync(contact, subject, body);

            await DisplayAlert("Preview",
                $"To: {message.EmailTo}\nSubject: {message.Subject}\n\n{message.Body}", "OK");
            DirectSendStatusLabel.Text = "Preview resolved with this lead's real values.";
        }
        catch (Exception ex)
        {
            DirectSendStatusLabel.Text = $"Preview failed: {ex.Message}";
        }
        finally
        {
            _isSendingDirect = false;
            DirectPreviewButton.IsEnabled = true;
            DirectSendIndicator.IsRunning = false;
            DirectSendIndicator.IsVisible = false;
        }
    }

    /// <summary>
    /// The direct send. Identical resolution order to the campaign loop on
    /// SendEmailsPage (Personalize → snapshot_random → snapshot_N →
    /// snapshot_ai → ToHtmlBody), gated by the same validator, with one
    /// recipient: this contact - or the test address when Test Email is on.
    /// </summary>
    private async void OnDirectSendClicked(object? sender, EventArgs e)
    {
        if (_isSendingDirect) return;
        if (BindingContext is not EmailContact contact) return;

        var subject = DirectSubjectEntry.Text?.Trim() ?? string.Empty;
        var body = DirectBodyEditor.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            DirectSendStatusLabel.Text = "Write both a subject and a message first.";
            return;
        }

        var isTest = TestEmailSwitch.IsToggled;
        var toAddress = isTest
            ? (TestEmailEntry.Text ?? string.Empty).Trim()
            : (DirectToEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(toAddress))
        {
            DirectSendStatusLabel.Text = isTest
                ? "Enter the address the test email should go to."
                : "This lead has no email address - fill in TO first.";
            return;
        }

        if (DirectAccountPicker.SelectedIndex < 0 || _directAccounts.Count == 0)
        {
            DirectSendStatusLabel.Text = "No active email account. Add one in Settings → Email Accounts.";
            return;
        }

        _isSendingDirect = true;
        DirectSendButton.IsEnabled = false;
        DirectPreviewButton.IsEnabled = false;
        DirectSendIndicator.IsRunning = true;
        DirectSendIndicator.IsVisible = true;
        DirectSendStatusLabel.Text = isTest ? $"Sending test to {toAddress}…" : $"Sending to {toAddress}…";

        try
        {
            // GATEKEEPER: same check the campaign page runs, for one email.
            var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.SendEmails, 1);
            if (!verdict.Approved)
            {
                DirectSendStatusLabel.Text = "Sending was not approved for this account.";
                return;
            }

            var message = await BuildDirectEmailAsync(contact, subject, body);
            message.EmailTo = toAddress;
            message.ToName = isTest ? toAddress : (contact.Name ?? contact.Email);

            var success = await _email.SendEmailAsync(message);

            if (!success)
            {
                DirectSendStatusLabel.Text = "Send failed. Check the account's SMTP settings and try again.";
                return;
            }

            if (isTest)
            {
                DirectSendStatusLabel.Text = $"Test email sent to {toAddress}. This lead was not emailed.";
            }
            else
            {
                // Only a real send counts against the lead's history.
                contact.LastEmailed = DateTime.Now;
                if (contact.Id == 0)
                    await _db.AddContactAsync(contact);
                else
                    await _db.UpdateContactAsync(contact);

                DirectSendStatusLabel.Text = $"Email sent to {toAddress}. Last emailed set to now.";
            }
        }
        catch (Exception ex)
        {
            DirectSendStatusLabel.Text = $"Send failed: {ex.Message}";
        }
        finally
        {
            _isSendingDirect = false;
            DirectSendButton.IsEnabled = true;
            DirectPreviewButton.IsEnabled = true;
            DirectSendIndicator.IsRunning = false;
            DirectSendIndicator.IsVisible = false;
        }
    }

    /// <summary>
    /// Resolves the raw subject/message into a ready-to-send MessengerDto for
    /// this contact. Shared by Preview and Send so the two can never drift.
    /// The To-address is left as the contact's own; the send handler overrides
    /// it when Test Email mode redirects the message.
    /// </summary>
    private async Task<MessengerDto> BuildDirectEmailAsync(
        EmailContact contact,
        string subject,
        string body)
    {
        var senderAccount = DirectAccountPicker.SelectedIndex >= 0 &&
                            DirectAccountPicker.SelectedIndex < _directAccounts.Count
            ? _directAccounts[DirectAccountPicker.SelectedIndex]
            : _directAccounts.FirstOrDefault();

        if (senderAccount is null)
            throw new InvalidOperationException("No active email account is configured.");

        var parameters = await _db.GetMessageParametersAsync();

        // [icebreaker] resolves from this lead's latest generated opener.
        var icebreaker = string.Empty;
        if (contact.Id > 0)
        {
            var openers = await _db.GetOpenersForLeadAsync(contact.Id);
            icebreaker = openers
                .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.Text.Trim())
                .FirstOrDefault() ?? string.Empty;
        }

        var personalizedSubject = EmailService.Personalize(subject, contact, parameters, icebreaker);
        var bodyText = EmailService.Personalize(body, contact, parameters, icebreaker);

        // Same order as the campaign loop: free tokens first, the vision call
        // only when the body actually asks for it.
        if (EmailService.ContainsSnapshotRandomToken(bodyText))
            bodyText = EmailService.PersonalizeSnapshotRandom(bodyText, contact.VideoSnapshot);

        if (EmailService.ContainsSnapshotIndexToken(bodyText))
            bodyText = EmailService.PersonalizeSnapshotIndexed(bodyText, contact.VideoSnapshot);

        if (EmailService.ContainsSnapshotAiToken(bodyText))
            bodyText = await EmailService.PersonalizeSnapshotAiAsync(
                bodyText, contact, _ai, contact.VideoSnapshot);

        return new MessengerDto
        {
            EmailFrom = senderAccount.EmailAddress,
            FromName = senderAccount.Name,
            EmailTo = contact.Email,
            ToName = contact.Name ?? contact.Email,
            Subject = personalizedSubject,
            Body = EmailService.ToHtmlBody(bodyText),
            SmtpHost = senderAccount.SmtpHost,
            SmtpPort = senderAccount.SmtpPort,
            SmtpUser = senderAccount.SmtpUser,
            SmtpPassword = senderAccount.SmtpPassword
        };
    }

    /// <summary>
    /// Returns to the contact list. The page has no nav bar (AppShell hides them
    /// app-wide), so this button is the back affordance.
    /// </summary>
    private async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopAsync();

    // ------------------------------------------------------------------
    //  AI video review
    // ------------------------------------------------------------------

    /// <summary>
    /// Runs the AI review of the lead's video and shows the editing notes.
    ///
    /// The transcript is fetched on demand rather than stored with the lead: it
    /// is large, it is only needed here, and re-running the analysis must be
    /// able to pick up a transcript that was missing during extraction.
    /// </summary>
    private async void OnIdentifyImprovementsClicked(object? sender, EventArgs e)
    {
        if (_isGeneratingImprovements) return;
        if (BindingContext is not EmailContact contact) return;

        _isGeneratingImprovements = true;

        ActionImprovementsButton.IsEnabled = false;
        ActionImprovementsButton.Text = "Analysing…";
        // The whole card starts hidden, so reveal it as well as the panel:
        // otherwise the results would be written into a collapsed card and the
        // user would see nothing happen.
        ImprovementsCard.IsVisible = true;
        ImprovementsPanel.IsVisible = true;
        ImprovementsIndicator.IsRunning = true;
        ImprovementsIndicator.IsVisible = true;
        ImprovementsStatusLabel.IsVisible = true;
        ImprovementsStatusLabel.Text = "Reading the video transcript…";
        ImprovementsLabel.IsVisible = false;
        ImprovementsLabel.Text = string.Empty;

        try
        {
            var transcript = string.Empty;

            if (!string.IsNullOrWhiteSpace(contact.VideoUrl))
            {
                try
                {
                    transcript = await _transcript.ExtractTranscriptAsync(contact.VideoUrl);
                }
                catch
                {
                    // No captions is common and not an error - the analysis falls
                    // back to the title, description and frames.
                }
            }

            ImprovementsStatusLabel.Text = "Generating editing notes…";

            var advice = await _ai.GenerateVideoImprovementsAsync(
                contact,
                transcript,
                _snapshots.Select(s => s.Image).ToList());

            if (string.IsNullOrWhiteSpace(advice))
            {
                ImprovementsStatusLabel.IsVisible = true;
                ImprovementsStatusLabel.Text =
                    "Could not generate an analysis. The AI service may be unreachable - check your connection and try again.";
                return;
            }

            ImprovementsLabel.Text = advice;
            ImprovementsLabel.IsVisible = true;
            ImprovementsStatusLabel.IsVisible = false;

            Log($"Improvements generated for {contact.Email}: {advice.Length} chars");
        }
        catch (Exception ex)
        {
            ImprovementsStatusLabel.IsVisible = true;
            ImprovementsStatusLabel.Text = $"Could not generate an analysis: {ex.Message}";
        }
        finally
        {
            _isGeneratingImprovements = false;
            ActionImprovementsButton.IsEnabled = true;
            ActionImprovementsButton.Text = $"{(char)0xD83D}{(char)0xDD0D} Video Review";
            ImprovementsIndicator.IsRunning = false;
            ImprovementsIndicator.IsVisible = false;
        }
    }

    /// <summary>
    /// Appends one line to the app's startup log. The same file already records
    /// crashes and startup breadcrumbs, so there is a single place to look.
    /// </summary>
    private static void Log(string message)
    {
        try
        {
            var dir = Path.Combine(FileSystem.AppDataDirectory, "TubeMailGorillaUnlocked");
            Directory.CreateDirectory(dir);
            File.AppendAllText(
                Path.Combine(dir, "startup-crash.log"),
                $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never break the page.
        }
    }

    /// <summary>Swipe handler - keeps the counter and caption in step.</summary>
    private void OnSnapshotPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        UpdateSnapshotIndicator(e.CurrentPosition);
    }

    private void OnPreviousSnapshotClicked(object? sender, EventArgs e) => MoveSnapshot(-1);

    private void OnNextSnapshotClicked(object? sender, EventArgs e) => MoveSnapshot(1);

    private void MoveSnapshot(int delta)
    {
        if (_snapshots.Count == 0) return;

        var target = SnapshotsCarousel.Position + delta;

        // Clamp rather than wrap: a bounce off either end of a contact's
        // snapshots reads better than silently jumping to the other side.
        if (target < 0 || target >= _snapshots.Count) return;

        SnapshotsCarousel.Position = target;
        UpdateSnapshotIndicator((int)target);
    }

    private void UpdateSnapshotIndicator(int index)
    {
        if (index < 0 || index >= _snapshots.Count) return;

        SnapshotCounterLabel.Text = $"{index + 1} of {_snapshots.Count}";

        PreviousSnapshotButton.IsEnabled = index > 0;
        NextSnapshotButton.IsEnabled = index < _snapshots.Count - 1;
    }

    /// <summary>
    /// Opens the source video in the browser, seeked to the moment the frame on
    /// screen was captured - the snapshot is only evidence if you can check it
    /// against the actual video.
    /// </summary>
    private async void OnWatchSnapshotClicked(object? sender, EventArgs e)
    {
        var index = (int)SnapshotsCarousel.Position;
        if (index < 0 || index >= _snapshots.Count || string.IsNullOrWhiteSpace(_videoUrl)) return;

        var seconds = (int)Math.Round(_snapshots[index].Seconds);
        var separator = _videoUrl.Contains('?') ? '&' : '?';
        var url = $"{_videoUrl}{separator}t={seconds}s";

        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Could not open video", ex.Message, "OK");
        }
    }

    /// <summary>
    /// Packs this lead's snapshots into a ZIP and hands it to the share sheet,
    /// where the user can save it to Files/Drive or attach it to an email.
    /// MAUI has no "download to disk" verb, so the share sheet is the native
    /// equivalent - and it avoids inventing a file path and hoping the platform
    /// can write to it.
    /// </summary>
    private async void OnDownloadSnapshotsClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not EmailContact contact) return;

        if (_snapshots.Count == 0)
        {
            await DisplayAlert("No snapshots",
                "This lead has no captured frames to export. They are captured during extraction.",
                "OK");
            return;
        }

        var original = ActionDownloadButton.Text;
        ActionDownloadButton.IsEnabled = false;
        ActionDownloadButton.Text = "Packing…";

        try
        {
            var archive = SnapshotZipService.Build(contact);
            if (archive is null)
            {
                await DisplayAlert("Could not export",
                    "None of this lead's stored snapshots could be read. Re-run extraction to capture them again.",
                    "OK");
                return;
            }

            var (data, fileName) = archive.Value;

            // Written to the cache directory first: the share sheet needs a real
            // file path, and cache is the only location guaranteed writable and
            // disposable on every platform.
            var path = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(path, data);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"{contact.DisplayName} - video snapshots",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Could not export", ex.Message, "OK");
        }
        finally
        {
            ActionDownloadButton.IsEnabled = true;
            ActionDownloadButton.Text = original;
        }
    }

    /// <summary>
    /// Loads previously generated icebreakers for this lead so they can be
    /// reviewed (and regenerated) while editing the contact.
    /// </summary>
    private async Task LoadOpenersAsync()
    {
        if (BindingContext is not EmailContact contact || contact.Id == 0)
        {
            NoOpenersLabel.IsVisible = true;
            OpenersStack.IsVisible = false;
            return;
        }

        try
        {
            var openers = await _db.GetOpenersForLeadAsync(contact.Id);
            BindableLayout.SetItemsSource(OpenersStack, openers);
            OpenersStack.IsVisible = openers.Count > 0;
            NoOpenersLabel.IsVisible = openers.Count == 0;
        }
        catch
        {
            NoOpenersLabel.IsVisible = true;
            OpenersStack.IsVisible = false;
        }
    }

    /// <summary>
    /// Generates a fresh AI icebreaker for this lead and saves it as an
    /// Opener. If the contact hasn't been saved yet, it is saved first so
    /// the opener has a lead Id to attach to.
    /// </summary>
    private async void OnGenerateIcebreakerClicked(object? sender, EventArgs e) =>
        await GenerateIcebreakerAsync(useCustomPrompt: false);

    private async void OnCustomAIPromptClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomAIPromptEditor.Text))
        {
            await DisplayAlert("Custom AI Prompt", "Enter instructions describing the icebreaker you want.", "OK");
            CustomAIPromptEditor.Focus();
            return;
        }

        await GenerateIcebreakerAsync(useCustomPrompt: true);
    }

    private async Task GenerateIcebreakerAsync(bool useCustomPrompt)
    {
        if (_isGeneratingIcebreaker) return;
        if (BindingContext is not EmailContact contact) return;

        // A new contact must exist in the DB before we can attach an opener.
        if (contact.Id == 0)
        {
            contact.Name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text.Trim();
            contact.Email = (EmailEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                await DisplayAlert("Email required", "Please enter an email address before generating an icebreaker.", "OK");
                return;
            }
            contact.ExtractedAt = DateTime.Now;
            await _db.AddContactAsync(contact);
        }

        _isGeneratingIcebreaker = true;
        ActionIcebreakerButton.IsEnabled = false;
        CustomAIPromptButton.IsEnabled = false;
        ActionIcebreakerButton.Text = "Generating…";
        CustomAIPromptButton.Text = "Writing…";

        try
        {
            var icebreaker = await _ai.GenerateIcebreakerAsync(
                contact,
                useCustomPrompt ? CustomAIPromptEditor.Text ?? string.Empty : string.Empty);

            if (string.IsNullOrWhiteSpace(icebreaker))
            {
                await DisplayAlert("Generation failed",
                    "The AI service timed out or could not be reached. Check your internet connection and try again.", "OK");
                return;
            }

            await _db.SaveOpenerAsync(new Opener
            {
                EmailerId = contact.Id,
                Text = icebreaker,
                CreatedAt = DateTime.Now
            });

            await LoadOpenersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not generate icebreaker: {ex.Message}", "OK");
        }
        finally
        {
            _isGeneratingIcebreaker = false;
            ActionIcebreakerButton.IsEnabled = true;
            CustomAIPromptButton.IsEnabled = true;
            ActionIcebreakerButton.Text = "✨ Icebreaker";
            CustomAIPromptButton.Text = "Custom AI Prompt";
        }
    }

    private async void OnSaveOpenerClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Opener opener }) return;

        if (string.IsNullOrWhiteSpace(opener.Text))
        {
            await DisplayAlert("Icebreaker required", "Enter an icebreaker before saving.", "OK");
            return;
        }

        opener.CreatedAt = DateTime.Now;
        await _db.SaveOpenerAsync(opener);
        await LoadOpenersAsync();
    }

    private async void OnDeleteOpenerClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.BindingContext is not Opener opener) return;

        try
        {
            await _db.DeleteOpenerAsync(opener.Id);
            await LoadOpenersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete icebreaker: {ex.Message}", "OK");
        }
    }

        private async void OnSave(object? sender, EventArgs e)
    {
        if (BindingContext is not EmailContact contact) return;

        contact.Name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text.Trim();
        contact.Email = (EmailEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(contact.Email))
        {
            await DisplayAlert("Email required", "Please enter an email address.", "OK");
            return;
        }

        if (contact.Id == 0)
            await _db.AddContactAsync(contact);
        else
        {
            contact.UpdatedAt = DateTime.Now;
            await _db.UpdateContactAsync(contact);
        }
        await Navigation.PopAsync();
    }

        private async void OnDelete(object? sender, EventArgs e)
    {
        if (BindingContext is not EmailContact contact) return;

        var confirm = await DisplayAlert(
            "Delete contact?",
            $"Remove \"{contact.Email}\" from your contacts?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        await _db.DeleteContactAsync(contact.Id);
        await Navigation.PopAsync();
    }
}