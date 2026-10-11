using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TubeMailGorilla.Mvc.Services;

public sealed partial class ComfyUiImageGenerationService
{
    /// <summary>
    /// Queues an img2img graph over /prompt, waits for it, fetches /view.
    /// Graph: Checkpoint -&gt; CLIP x2 -&gt; LoadImage -&gt; ImageScale -&gt; VAEEncode -&gt;
    /// KSampler -&gt; VAEDecode -&gt; SaveImage.
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
        var w = Math.Max(8, (int)(width / 8) * 8);
        var h = Math.Max(8, (int)(height / 8) * 8);

        var graph = BuildGraph(checkpoint, imageName, prompt, w, h, seed, steps, denoise);
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

    private static Dictionary<string, object?> BuildGraph(
        string checkpoint, string imageName, string prompt, int w, int h, int seed, int steps, float denoise)
    {
        return new Dictionary<string, object?>
        {
            ["1"] = Node("CheckpointLoaderSimple",
                new Dictionary<string, object?> { ["ckpt_name"] = checkpoint }),
            ["2"] = Node("CLIPTextEncode",
                new Dictionary<string, object?>
                {
                    ["text"] = $"cinematic still, {prompt}",
                    ["clip"] = new object?[] { "1", 1 },
                }),
            ["3"] = Node("CLIPTextEncode",
                new Dictionary<string, object?>
                {
                    ["text"] = "blurry, watermark, text, logo, deformed, low quality",
                    ["clip"] = new object?[] { "1", 1 },
                }),
            ["4"] = Node("LoadImage",
                new Dictionary<string, object?> { ["image"] = imageName }),
            ["9"] = Node("ImageScale",
                new Dictionary<string, object?>
                {
                    ["image"] = new object?[] { "4", 0 },
                    ["width"] = w,
                    ["height"] = h,
                    ["upscale_method"] = "bilinear",
                    ["crop"] = "disabled",
                }),
            ["5"] = Node("VAEEncode",
                new Dictionary<string, object?>
                {
                    ["pixels"] = new object?[] { "9", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                }),
            ["6"] = Node("KSampler",
                new Dictionary<string, object?>
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
                }),
            ["7"] = Node("VAEDecode",
                new Dictionary<string, object?>
                {
                    ["samples"] = new object?[] { "6", 0 },
                    ["vae"] = new object?[] { "1", 2 },
                }),
            ["8"] = Node("SaveImage",
                new Dictionary<string, object?>
                {
                    ["images"] = new object?[] { "7", 0 },
                    ["filename_prefix"] = "tmg_snapshot_ai",
                }),
        };
    }

    private static Dictionary<string, object?> Node(string classType, Dictionary<string, object?> inputs) =>
        new() { ["class_type"] = classType, ["inputs"] = inputs };
}
