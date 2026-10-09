using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Renders [snapshot_ai] images through a local ComfyUI server (img2img):
/// takes the lead's picked frame, repaints it per the vision model's edit
/// prompt, returns ONE finished image. This is what makes the {instruction}
/// actually change the picture instead of compositing 4 thumbnails.
///
/// Unconfigured (empty ComfyUiBaseUrl) by default: IsConfigured is false and
/// GenerateAsync returns null, so AIService falls back to compositing real
/// frames. Set the URL once ComfyUI runs locally (default
/// http://127.0.0.1:8188) with an SD1.5/SDXL checkpoint in
/// ComfyUI/models/checkpoints.
///
/// Never throws for ordinary failures (server down, bad checkpoint, timeout):
/// returns null so the caller falls back to real frames.
/// </summary>
public sealed class ComfyUiImageGenerationService : IImageGenerationService
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

    /// <summary>
    /// Queues an img2img graph over /prompt, waits for it, fetches /view.
    /// Graph: Checkpoint -> CLIP x2 -> LoadImage -> ImageScale -> VAEEncode ->
    /// KSampler -> VAEDecode -> SaveImage. ImageScale runs the source frame to
    /// the requested size first, so the output honours the email display size.
    /// </summary>
    private async Task<byte[]?> QueueImg2ImgAsync(
        string baseUrl, string imageName, string prompt, int width, int height, CancellationToken token)
    {
        var checkpoint = (_settings.ComfyUiCheckpoint ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(checkpoint))
            return null;

        var denoise = Math.Clamp(_settings.ComfyUiDenoise, 0.05f, 1f);
        var steps = Math.Clamp(_settings.ComfyUiSteps, 1, 150);
        var seed = Random.Shared.Next(1, int.MaxValue);

        // SD works in 8-pixel latent blocks; snap to a multiple of 8 so the VAE
        // does not pad or reject the dimensions.
        var w = (int)(width / 8) * 8;
        var h = (int)(height / 8) * 8;

        var graph = new Dictionary<string, object?>
        {
            ["1"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CheckpointLoaderSimple",
                ["inputs"] = new Dictionary<string, object?> { ["ckpt_name"] = checkpoint },
            },
            ["2"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["text"] = $"cinematic still, {prompt}",
                    ["clip"] = new object?[] { "1", 1 },
                },
            },
            ["3"] = new Dictionary<string, object?>
            {
                ["class_type"] = "CLIPTextEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["text"] = "blurry, watermark, text, logo, deformed, low quality",
                    ["clip"] = new object?[] { "1", 1 },
                },
            },
            ["4"] = new Dictionary<string, object?>
            {
                ["class_type"] = "LoadImage",
                ["inputs"] = new Dictionary<string, object?> { ["image"] = imageName },
            },
            ["9"] = new Dictionary<string, object?>
            {
                ["class_type"] = "ImageScale",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["image"] = new object?[] { "4", 0 },
                    ["width"] = w,
                    ["height"] = h,
                    ["upscale_method"] = "bilinear",
                    ["crop"] = "disabled",
                },
            },
            ["5"] = new Dictionary<string, object?>
            {
                ["class_type"] = "VAEEncode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["pixels"] = new object?[] { "9", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                },
            },
            ["6"] = new Dictionary<string, object?>
            {
                ["class_type"] = "KSampler",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["model"] = new object?[] { "1", 0 },
                    ["positive"] = new object?[] { "2", 0 },
                    ["negative"] = new object?[] { "3", 0 },
                    ["latent_image"] = new object?[] { "5", 0 },
                    ["seed"] = seed,
                    ["steps"] = steps,
                    ["cfg"] = 8.0,
                    ["sampler_name"] = "euler_ancestral",
                    ["scheduler"] = "normal",
                    ["denoise"] = denoise,
                },
            },
            ["7"] = new Dictionary<string, object?>
            {
                ["class_type"] = "VAEDecode",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["samples"] = new object?[] { "6", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                },
            },
            ["8"] = new Dictionary<string, object?>
            {
                ["class_type"] = "SaveImage",
                ["inputs"] = new Dictionary<string, object?>
                {
                    ["images"] = new object?[] { "7", 0 },
                    ["filename_prefix"] = "tmg_snapshot_ai",
                },
            },
        };

        var payload = JsonSerializer.Serialize(new { prompt = graph }, JsonOptions);
        using var queueResponse = await Http.PostAsync(
            $"{baseUrl}/prompt",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            token);
        if (!queueResponse.IsSuccessStatusCode)
            return null;

        var queueJson = await queueResponse.Content.ReadAsStringAsync(token);
        var queued = JsonSerializer.Deserialize<ComfyQueueResponse>(queueJson, JsonOptions);
        if (string.IsNullOrWhiteSpace(queued?.PromptId))
            return null;

        var output = await WaitForOutputAsync(baseUrl, queued!.PromptId!, token);
        if (output is null)
            return null;

        var viewUrl = $"{baseUrl}/view?filename={Uri.EscapeDataString(output.Filename)}" +
                      $"&subfolder={Uri.EscapeDataString(output.Subfolder)}" +
                      $"&type={Uri.EscapeDataString(output.Type)}";
        using var imageResponse = await Http.GetAsync(viewUrl, token);
        if (!imageResponse.IsSuccessStatusCode)
            return null;

        return await imageResponse.Content.ReadAsByteArrayAsync(token);
    }

    /// <summary>
    /// Waits for a queued prompt to finish by polling /history/{prompt_id}
    /// until ComfyUI records an output image, then returns its fetch info.
    ///
    /// Polling (rather than a websocket) keeps this simple and reliable on a
    /// MAUI client: no client-id handshake, and it survives the server briefly
    /// dropping the socket mid-run. A missing/empty history entry just means
    /// "not done yet", so it retries until the timeout budget is spent.
    /// </summary>
    private async Task<ComfyOutputImage?> WaitForOutputAsync(
        string baseUrl, string promptId, CancellationToken token)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(Math.Max(30, _settings.ComfyUiTimeoutSeconds));
        var historyUrl = $"{baseUrl}/history/{Uri.EscapeDataString(promptId)}";

        while (DateTime.UtcNow < deadline)
        {
            token.ThrowIfCancellationRequested();
            HttpResponseMessage response;
            try
            {
                response = await Http.GetAsync(historyUrl, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                await Task.Delay(1000, token);
                continue;
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(token);
                    var history = JsonSerializer.Deserialize<Dictionary<string, ComfyHistoryEntry>>(json, JsonOptions);
                    var image = history?.Values
                        .Where(e => e.Outputs is not null)
                        .SelectMany(e => e.Outputs!.Values)
                        .Where(n => n.Images is not null)
                        .SelectMany(n => n.Images!)
                        .FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Filename));

                    if (image is not null)
                        return image;
                }
            }

            await Task.Delay(750, token);
        }

        return null;
    }

    // ---- ComfyUI JSON shapes -------------------------------------------------
    // Only the fields the client reads are declared; the rest of ComfyUI's
    // verbose payload is ignored by the serializer.

    private sealed class ComfyQueueResponse
    {
        [JsonPropertyName("prompt_id")]
        public string? PromptId { get; set; }
    }

    private sealed class ComfyUploadResponse
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class ComfyHistoryEntry
    {
        [JsonPropertyName("outputs")]
        public Dictionary<string, ComfyNodeOutput>? Outputs { get; set; }
    }

    private sealed class ComfyNodeOutput
    {
        [JsonPropertyName("images")]
        public List<ComfyOutputImage>? Images { get; set; }
    }

    private sealed class ComfyOutputImage
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; } = string.Empty;

        [JsonPropertyName("subfolder")]
        public string Subfolder { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
