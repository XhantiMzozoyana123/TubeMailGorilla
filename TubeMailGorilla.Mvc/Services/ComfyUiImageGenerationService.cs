using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Renders [snapshot_ai] images through a local ComfyUI server (img2img):
/// takes the lead's picked frame, repaints it per the vision model's edit
/// prompt, returns ONE finished image. Unconfigured (empty ComfyUiBaseUrl):
/// IsConfigured is false and GenerateAsync returns null, so AIService falls
/// back to compositing real frames. Never throws for ordinary failures.
/// </summary>
public sealed partial class ComfyUiImageGenerationService : IImageGenerationService
{
    private static readonly HttpClient Http = new() { Timeout = Timeout.InfiniteTimeSpan };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly LlmSettings _settings;

    public ComfyUiImageGenerationService(LlmSettings settings)
    {
        _settings = settings;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.ComfyUiBaseUrl);

    public async Task<string?> GenerateAsync(
        IReadOnlyList<string> referenceImages,
        string editPrompt,
        int width,
        int height,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = (_settings.ComfyUiBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return null;

        var first = referenceImages?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        if (string.IsNullOrWhiteSpace(first))
            return null;

        byte[] sourceBytes;
        try
        {
            sourceBytes = Convert.FromBase64String(CleanBase64(first!));
        }
        catch
        {
            return null;
        }

        if (sourceBytes.Length == 0)
            return null;

        var prompt = string.IsNullOrWhiteSpace(editPrompt)
            ? "subtle professional color grade, clean studio look"
            : editPrompt.Trim();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(30, _settings.ComfyUiTimeoutSeconds)));

        try
        {
            var uploadedName = await UploadImageAsync(baseUrl, sourceBytes, timeout.Token);
            if (uploadedName is null)
                return null;

            var imageBytes = await QueueImg2ImgAsync(
                baseUrl, uploadedName, prompt, width, height, timeout.Token);
            if (imageBytes is null || imageBytes.Length == 0)
                return null;

            return Convert.ToBase64String(imageBytes);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string CleanBase64(string value)
    {
        var comma = value.IndexOf(',');
        if (value.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            value = value[(comma + 1)..];
        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>POSTs the source frame to /upload/image, returns its server name.</summary>
    private static async Task<string?> UploadImageAsync(
        string baseUrl, byte[] pngOrJpeg, CancellationToken token)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            using var content = new ByteArrayContent(pngOrJpeg);
            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(content, "image", "snapshot.jpg");

            using var response = await Http.PostAsync($"{baseUrl}/upload/image", form, token);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(token);
            var upload = JsonSerializer.Deserialize<ComfyUploadResponse>(json, JsonOptions);
            return string.IsNullOrWhiteSpace(upload?.Name) ? null : upload!.Name;
        }
        catch
        {
            return null;
        }
    }
}
