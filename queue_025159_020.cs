=== lines 96-158 (upload/queue head) ===
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
=== lines 250-280 (queue tail) ===
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
