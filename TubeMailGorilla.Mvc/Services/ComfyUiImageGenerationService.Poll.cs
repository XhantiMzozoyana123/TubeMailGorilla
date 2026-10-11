using System.Text.Json;
using System.Text.Json.Serialization;

namespace TubeMailGorilla.Mvc.Services;

public sealed partial class ComfyUiImageGenerationService
{
    /// <summary>
    /// Waits for a queued prompt to finish by polling /history/{prompt_id}
    /// until ComfyUI records an output image, then returns its fetch info.
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
