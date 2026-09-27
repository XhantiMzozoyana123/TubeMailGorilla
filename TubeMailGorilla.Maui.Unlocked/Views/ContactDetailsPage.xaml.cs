using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

public partial class ContactDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly YouTubeTranscriptService _transcript;
    private bool _isGeneratingIcebreaker;
    private bool _isGeneratingImprovements;

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
        BindingContext = contact;
        NameEntry.Text = contact.Name ?? string.Empty;
        EmailEntry.Text = contact.Email;
        LoadSnapshots(contact);
        _ = LoadOpenersAsync();
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

        if (!hasSnapshots) return;

        SnapshotsCarousel.ItemsSource = _snapshots;
        SnapshotsCarousel.Position = 0;
        UpdateSnapshotIndicator(0);
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
            ActionImprovementsButton.Text = "&#x1F50D; Video Review";
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
            ActionIcebreakerButton.Text = "&#x2728; Icebreaker";
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