using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class ExtractPage : ContentPage
{
    private readonly ExtractService _extract;
    private readonly PaymentService _payments;
    private readonly ValidationService _validator;
    private readonly LLMService _llm;
    private EntitlementInfo _entitlements = new();

    public ExtractPage()
    {
        InitializeComponent();
        _extract = ServiceHelper.GetService<ExtractService>();
        _payments = ServiceHelper.GetService<PaymentService>();
        _validator = ServiceHelper.GetService<ValidationService>();
        _llm = ServiceHelper.GetService<LLMService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Server-authoritative plan limits (fail closed to FREE limits).
        _entitlements = await _payments.GetEntitlementsAsync();

        StartButton.Text = _entitlements.IsSubscribed
            ? "Start Extraction"
            : "Start Extraction (Free: 1 run / month)";
    }

    private async void OnStartExtractionClicked(object? sender, EventArgs e)
    {
        var keyword = KeywordEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(keyword))
        {
            StatusLabel.Text = "Please enter a keyword.";
            return;
        }

        var pageLimit = 5;
        if (int.TryParse(PageLimitEntry.Text, out var parsed) && parsed > 0)
            pageLimit = parsed;

        // GATEKEEPER: the API must approve this extraction BEFORE any work.
        var verdict = await _validator.CheckOrAlertAsync(this, ValidationService.ExtractLeads, pageLimit);
        if (!verdict.Approved) return;

        // Respect the server-approved limit (never trust local settings).
        var cappedByPlan = false;
        if (verdict.Limit >= 0 && pageLimit > verdict.Limit)
        {
            pageLimit = verdict.Limit;
            cappedByPlan = true;
        }

        StatusLabel.Text = "Extracting...";
        ResultsLabel.Text = string.Empty;
        ExtractionIndicator.IsVisible = true;
        StartButton.IsEnabled = false;

        // The on-device AI model used to be downloaded/loaded lazily on the first
        // inference mid-loop (no progress was reported until a full video finished),
        // which made the UI freeze on a fixed percentage ("20%"). Prepare it upfront
        // with live status, or continue without AI fields if it can't be made ready.
        if (!await EnsureAiModelReadyAsync())
            StatusLabel.Text = $"AI model unavailable ({_llm.Status}) - continuing without AI fields.";

        var progress = new Progress<int>(p =>
        {
            // Surface LLM status (model download/loading) so a long first inference
            // never looks like the app has frozen.
            var llmStatus = _llm.Status;
            StatusLabel.Text = (string.IsNullOrEmpty(llmStatus) ||
                                llmStatus.StartsWith("LLM not", StringComparison.Ordinal) ||
                                llmStatus == "Model ready.")
                ? $"Extracting... {p}%"
                : $"Extracting ({llmStatus})... {p}%";
        });
        var result = await _extract.ExtractByKeywordAsync(
            keyword,
            pageLimit,
            SendSettings.ExtractGmailOnly,
            SendSettings.ExtractValidateEmails,
            progress);

        StatusLabel.Text = "Done.";
        ResultsLabel.Text = $"Videos: {result.TotalVideos}  |  Emails: {result.EmailsFound}  |  Errors: {result.Errors}";
        ExtractionIndicator.IsVisible = false;
        StartButton.IsEnabled = true;

        if (cappedByPlan)
        {
            ResultsLabel.Text += $"\n\nYour plan is limited to {verdict.Limit} leads per extraction. Upgrade to Pro on the Subscription tab for more.";
        }
    }

    // ------------------------------------------------------------------
    //  BULK EXTRACTION (CSV)
    // ------------------------------------------------------------------

    /// <summary>A single row of the bulk extraction CSV.</summary>
    private sealed record BulkRow(string Keyword, int PageLimit);

    private async void OnDownloadTemplateClicked(object? sender, EventArgs e)
    {
        try
        {
            var template =
                "keyword,pages\n" +
                "marketing,5\n" +
                "tech review,3\n" +
                "\"video editing services\",5\n" +
                "fitness coach,2\n";

            // Let the user pick exactly where to save it.
            var fullPath = await AskUserForTemplateSavePathAsync();
            if (fullPath is null) return; // user cancelled

            await File.WriteAllTextAsync(fullPath, template, System.Text.Encoding.UTF8);

            OpenFolderInFileBrowser(Path.GetDirectoryName(fullPath) ?? string.Empty);
            BulkStatusLabel.Text = $"Template saved to:\n{fullPath}\n\nOpen it, swap in your own keywords (one per line), save it, then hit \"Upload CSV & Start Bulk Extracting\".";
        }
        catch (Exception ex)
        {
            BulkStatusLabel.Text = $"Hmm, we couldn't save the template: {ex.Message}";
        }
    }

    /// <summary>
    /// Asks the user where to save the template CSV.
    /// Windows shows a native Save-As dialog (defaulting to Desktop);
    /// other platforms fall back to a sensible default location.
    /// Returns null if the user cancels.
    /// </summary>
    private async Task<string?> AskUserForTemplateSavePathAsync()
    {
        var defaultName = $"TubeMailGorilla-Bulk-Template-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

#if WINDOWS
        try
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop,
                SuggestedFileName = defaultName,
            };
            picker.FileTypeChoices.Add("CSV file", new List<string> { ".csv" });

            // Unpackaged apps must hand the picker our window handle (HWND).
            var platformWindow = (Microsoft.UI.Xaml.Window?)App.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView;
            if (platformWindow is null)
                return Path.Combine(FileSystem.AppDataDirectory, defaultName);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            return file?.Path; // null when cancelled
        }
        catch (Exception ex)
        {
            // Picker unavailable for any reason -> fall back to app data,
            // but tell the user exactly where it went.
            var fallback = Path.Combine(FileSystem.AppDataDirectory, defaultName);
            BulkStatusLabel.Text = $"The save dialog couldn't open ({ex.Message}).\nTemplate will be saved to:\n{fallback}";
            return fallback;
        }
#else
        return Path.Combine(FileSystem.AppDataDirectory, defaultName);
#endif
    }

    private async void OnBulkExtractionClicked(object? sender, EventArgs e)
    {
        // Pick the CSV file.
        var pickOptions = new PickOptions
        {
            PickerTitle = "Select your bulk extraction CSV",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = new[] { ".csv", ".txt" },
                [DevicePlatform.MacCatalyst] = new[] { "public.comma-separated-values-text", "public.plain-text" },
            })
        };

        var file = await FilePicker.PickAsync(pickOptions);
        if (file is null) return; // user cancelled

        List<BulkRow> rows;
        try
        {
            rows = ParseBulkCsv(await File.ReadAllTextAsync(file.FullPath ?? string.Empty));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Could not read CSV", ex.Message, "OK");
            return;
        }

        if (rows.Count == 0)
        {
            await DisplayAlert("That file came back empty",
                "We couldn't find any keywords in it.\n\nIt should look like this:\n\nmarketing,5\ntech review,3\n\nTip: tap \"Download CSV Bulk Extraction Template\" first — it gives you a ready-made file to fill in.", "Got it");
            return;
        }

        // GATEKEEPER: Bulk extraction is a Pro feature. Free users are blocked
        // outright before any work begins (the server whitelists this action only
        // for paying users).
        var bulkVerdict = await _validator.CheckOrAlertAsync(this, ValidationService.BulkExtractLeads, 0);
        if (!bulkVerdict.Approved)
            return;

        SetBusy(isBusy: true);
        BulkStatusLabel.Text = string.Empty;

        // Same upfront model prep as single extraction, so a first-run download/load
        // can never stall the middle of a bulk run with no visible progress.
        if (!await EnsureAiModelReadyAsync())
            BulkStatusLabel.Text = $"AI model unavailable ({_llm.Status}) - running without AI fields.";

        var totalVideos = 0;
        var totalEmails = 0;
        var totalErrors = 0;
        var skipped = 0;

        try
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var prefix = $"[{i + 1}/{rows.Count}] \"{row.Keyword}\" — ";

                // GATEKEEPER: every keyword must be approved by the API first.
                var verdict = await _validator.CheckAsync(ValidationService.ExtractLeads, row.PageLimit);
                if (!verdict.Approved)
                {
                    skipped++;
                    BulkStatusLabel.Text = $"{prefix}skipped — your plan said not this time.\n{verdict.Reason}";
                    continue;
                }

                // Respect the server-approved limit (never trust the CSV).
                var pageLimit = row.PageLimit;
                if (verdict.Limit >= 0 && pageLimit > verdict.Limit)
                    pageLimit = verdict.Limit;

                BulkStatusLabel.Text = $"{prefix}extracting...";

                // Combined progress across all keywords.
                var progress = new Progress<int>(p =>
                {
                    int overall = (i * 100 + Math.Clamp(p, 0, 100)) / rows.Count;
                    BulkStatusLabel.Text = $"{prefix}extracting... {p}%   (overall {overall}%)";
                });

                var result = await _extract.ExtractByKeywordAsync(
                    row.Keyword,
                    pageLimit,
                    SendSettings.ExtractGmailOnly,
                    SendSettings.ExtractValidateEmails,
                    progress);

                totalVideos += result.TotalVideos;
                totalEmails += result.EmailsFound;
                totalErrors += result.Errors;
            }

            BulkStatusLabel.Text =
                $"Bulk extract complete.\nKeywords run: {rows.Count - skipped}/{rows.Count}" +
                (skipped > 0 ? $"  ({skipped} skipped by plan)" : "") +
                $"\nTotal videos: {totalVideos}  |  Total emails: {totalEmails}  |  Errors: {totalErrors}" +
                (totalEmails > 0 ? "\n\nYour fresh leads are waiting on the Contacts page." : "");

            ResultsLabel.Text = $"Videos: {totalVideos}  |  Emails: {totalEmails}  |  Errors: {totalErrors}";
        }
        catch (Exception ex)
        {
            BulkStatusLabel.Text = $"The Gorilla tripped over something: {ex.Message}\n" +
                                   $"Progress so far — Videos: {totalVideos}, Emails: {totalEmails}.";
        }
        finally
        {
            SetBusy(isBusy: false);
        }
    }

    /// <summary>
    /// Parses the bulk CSV. Accepts an optional header row and both
    /// "keyword,pages" and bare "keyword" lines (pages defaults to 5).
    /// Handles double-quoted keywords containing commas; '#' lines are comments.
    /// </summary>
    private static List<BulkRow> ParseBulkCsv(string content)
    {
        var rows = new List<BulkRow>();

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim().TrimEnd('\r');
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var fields = SplitCsvLine(line);
            if (fields.Length == 0) continue;

            var keyword = fields[0].Trim();
            if (keyword.Length == 0) continue;

            // Skip a header row like "keyword,pages".
            if (rows.Count == 0 && keyword.Equals("keyword", StringComparison.OrdinalIgnoreCase))
                continue;

            var pages = 5;
            if (fields.Length > 1 && int.TryParse(fields[1].Trim(), out var parsed) && parsed > 0)
                pages = parsed;

            rows.Add(new BulkRow(keyword, pages));
        }

        return rows;
    }

    /// <summary>Splits one CSV line, honoring double-quoted fields.</summary>
    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());

        return fields.ToArray();
    }

    /// <summary>Opens the folder in Explorer/Finder so users can find their template.</summary>
    private void OpenFolderInFileBrowser(string folder)
    {
#if WINDOWS
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true,
            });
        }
        catch { /* non-fatal */ }
#endif
    }

    /// <summary>
    /// Ensures the on-device LLM model is downloaded and loaded before an extraction
    /// loop starts. While the model is being prepared, this keeps the status line live
    /// (download %, "Loading model...") so the UI never looks frozen on a fixed
    /// percentage. Returns false when the model can't be made ready - AI fields are then
    /// left empty (AIService fails gracefully) but scraping still proceeds.
    /// </summary>
    private async Task<bool> EnsureAiModelReadyAsync()
    {
        if (_llm.IsReady)
            return true;

        var readyTask = _llm.EnsureReadyAsync();
        while (!readyTask.IsCompleted)
        {
            StatusLabel.Text = $"Preparing AI model ({_llm.Status})...";
            await Task.Delay(250);
        }
        return await readyTask && _llm.IsReady;
    }

    private void SetBusy(bool isBusy)
    {
        BulkButton.IsEnabled = !isBusy;
        DownloadTemplateButton.IsEnabled = !isBusy;
        StartButton.IsEnabled = !isBusy;
        ExtractionIndicator.IsVisible = isBusy;
        ExtractionIndicator.IsRunning = isBusy;
    }
}