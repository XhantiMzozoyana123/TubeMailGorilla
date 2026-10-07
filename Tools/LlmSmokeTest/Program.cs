using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace LlmSmokeTest;

/// <summary>
/// Standalone smoke test for the MAUI app's LlamaSharp LLM pipeline.
///
/// Compiles the REAL Services/LLMService.cs + Models/LlmSettings.cs sources
/// and the REAL appsettings.json, then runs:
///   1. model load (chat GGUF + vision projector) - the exact path the app takes
///   2. a text-extraction call (what ExtractService/AIService use)
///   3. an advisory call (what SnapshotService/ExtractService use)
///   4. a vision call (text + a generated JPEG through the projector)
///
/// If all four pass, the app's LLM is working. Usage:
///   dotnet run --project Tools\LlmSmokeTest
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var failures = 0;

        // Same binding the app performs in MauiProgram.cs.
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var settings = new LlmSettings();
        config.GetSection(nameof(LlmSettings)).Bind(settings);

        using var llm = new LLMService(settings);
        Console.WriteLine($"Chat model file : {llm.ChatModelFile}");
        Console.WriteLine($"Vision file     : {llm.VisionModel}");
        Console.WriteLine($"Model directory : {llm.ModelDirectory}");
        Console.WriteLine($"Context size    : {settings.ContextSize}, GPU layers: {settings.GpuLayerCount}");
        Console.WriteLine();

        ReportModelFile(llm.ChatModelFile, "chat model");
        ReportModelFile(llm.VisionModel, "vision projector");
        Console.WriteLine();

        // 1. Load: the same EnsureReadyAsync path the app uses.
        failures += await Check("1. Model load", async () =>
        {
            Console.WriteLine("   Loading weights (first load can take 30-90s, mostly GPU upload)...");
            var sw = Stopwatch.StartNew();
            var ready = await llm.EnsureReadyAsync();
            sw.Stop();
            Console.WriteLine($"   EnsureReadyAsync -> {ready} in {sw.Elapsed.TotalSeconds:N1}s");
            Console.WriteLine($"   Status: {llm.Status}");
            if (!ready) throw new Exception($"Load failed: {llm.Status}");
            if (!llm.IsReady) throw new Exception("IsReady is false after a successful load.");
        });

        if (!llm.IsReady)
        {
            Console.WriteLine("\nSMOKE TEST FAILED: model did not load - skipping inference checks.");
            return 1;
        }

        // 2. Text extraction (what ExtractService/AIService call).
        failures += await Check("2. Text extraction (ExtractFieldValueAsync)", async () =>
        {
            var sw = Stopwatch.StartNew();
            var reply = await llm.ExtractFieldValueAsync(
                "FULL NAME",
                "Hey everyone, I'm Sarah Mitchell, and today we're taking a tour of my studio where I run CraftCo.",
                maxTokens: 60);
            sw.Stop();
            Console.WriteLine($"   ({sw.Elapsed.TotalSeconds:N1}s) Reply: '{reply.Trim()}'");
            if (reply.StartsWith("LLM Error", StringComparison.Ordinal))
                throw new Exception(reply);
            if (!reply.Contains("Sarah Mitchell", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Unexpected answer - expected 'Sarah Mitchell', got '{reply.Trim()}'.");
        });

        // 3. Advisory-style call (longer free-text answer).
        failures += await Check("3. Advisory generation (GenerateAdvisoryAsync)", async () =>
        {
            var sw = Stopwatch.StartNew();
            var reply = await llm.GenerateAdvisoryAsync(
                "Give ONE specific editing tip for this video, in one sentence. " +
                "Video: 'I Edited 100 Videos - Here Is What I Learned' by CutCraft. " +
                "Transcript: 'Welcome back. Today I am breaking down the five cuts that doubled my retention. " +
                "First, the cold open. No intro, no logo sting, straight into the payoff.'",
                maxTokens: 256);
            sw.Stop();
            Console.WriteLine($"   ({sw.Elapsed.TotalSeconds:N1}s) Reply ({reply.Trim().Length} chars):");
            Console.WriteLine($"   {reply.Trim()}");
            if (reply.StartsWith("LLM Error", StringComparison.Ordinal))
                throw new Exception(reply);
        });

        // 4. Vision: send one real JPEG through the projector.
        failures += await Check("4. Vision projector (GenerateTextAsync + image)", async () =>
        {
            if (!llm.SupportsVision)
            {
                Console.WriteLine("   SKIPPED: projector did not load (Status: " + llm.Status + ").");
                return;
            }

            var jpeg = CreateTestJpeg();
            var sw = Stopwatch.StartNew();
            var reply = await llm.GenerateTextAsync(
                "Look at the image and describe what you see in one short sentence.",
                maxTokens: 120,
                systemPrompt: "You are a helpful vision assistant. Describe the image briefly in one short sentence. Plain text only.",
                base64Images: new[] { jpeg });
            sw.Stop();
            Console.WriteLine($"   ({sw.Elapsed.TotalSeconds:N1}s) Reply: '{reply.Trim()}'");
            if (reply.StartsWith("LLM Error", StringComparison.Ordinal))
                throw new Exception(reply);
            if (string.IsNullOrWhiteSpace(reply))
                throw new Exception("Empty reply from the vision call.");
        });

        Console.WriteLine();
        Console.WriteLine(failures == 0 ? "SMOKE TEST PASSED" : $"SMOKE TEST FAILED ({failures} check(s) failed)");
        return failures == 0 ? 0 : 1;
    }

    private static async Task<int> Check(string name, Func<Task> body)
    {
        Console.WriteLine($"---- {name} ----");
        try
        {
            await body();
            Console.WriteLine("   PASS\n");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   FAIL: {ex.Message}\n");
            return 1;
        }
    }

    private static void ReportModelFile(string path, string label)
    {
        if (File.Exists(path))
            Console.WriteLine($"{label,-17}: FOUND ({new FileInfo(path).Length / (1024.0 * 1024.0):N1} MB)");
        else
            Console.WriteLine($"{label,-17}: MISSING at {path}");
    }

    /// <summary>
    /// Builds a small synthetic JPEG (a red/blue split frame with a gold
    /// circle) so the vision check has something real to encode - no fixture
    /// files needed.
    /// </summary>
    private static string CreateTestJpeg()
    {
        using var surface = SKSurface.Create(new SKImageInfo(320, 180));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.CornflowerBlue);
        using (var paint = new SKPaint { Color = SKColors.Firebrick })
            canvas.DrawRect(0, 0, 160, 180, paint);
        using (var paint = new SKPaint { Color = SKColors.Gold, IsAntialias = true })
            canvas.DrawCircle(240, 90, 45, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var bytes = data.ToArray();
        Console.WriteLine($"   Test JPEG: {bytes.Length / 1024} KB");
        return Convert.ToBase64String(bytes);
    }
}