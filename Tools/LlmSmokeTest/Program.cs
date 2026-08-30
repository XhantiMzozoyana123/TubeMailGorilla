using System.Diagnostics;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace LlmSmokeTest;

/// <summary>
/// Standalone smoke test for the TubeMailGorilla MAUI LLM pipeline.
///
/// It loads the SAME bundled GGUF model + uses the SAME LLamaSharp 0.27.0 API and
/// StatelessExecutor settings as Services\LLMService.cs in the MAUI app
/// (ApplyTemplate = true, a strict extraction SystemMessage, DefaultSamplingPipeline,
/// MaxTokens, and a Llama 3 end-of-turn anti-prompt). If this produces sensible output,
/// the packaged model and the app's inference path are working.
///
/// Usage:
///   dotnet run --project Tools\LlmSmokeTest -- --model <path-to-.gguf>
/// </summary>
public static class Program
{
    private const string SYSTEM_PROMPT =
        "You are a strict data-extraction engine. " +
        "Return ONLY the raw data value the user asks for. " +
        "NEVER summarize, explain, or add context. " +
        "Your ENTIRE response must be the single data value and nothing else. " +
        "If the data cannot be found, output nothing at all.";

    public static async Task<int> Main(string[] args)
    {
        var modelPath = ResolveModelPath(args);
        if (modelPath is null)
        {
            Console.WriteLine("Model not found. Pass --model <path> or place a .gguf next to the app.");
            return 2;
        }
        Console.WriteLine($"Model: {modelPath}");
        Console.WriteLine($"Size : {new FileInfo(modelPath).Length / (1024.0 * 1024.0):N1} MB");

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 4096,
            GpuLayerCount = 0
        };

        try
        {
            Console.WriteLine("Loading weights (first load can take ~30-90s on CPU)...");
            var sw = Stopwatch.StartNew();
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);
            sw.Stop();
            Console.WriteLine($"Loaded in {sw.Elapsed.TotalSeconds:N1}s.");

            var executor = new StatelessExecutor(model, parameters)
            {
                ApplyTemplate = true,
                SystemMessage = SYSTEM_PROMPT
            };

            var inferenceParams = new InferenceParams
            {
                SamplingPipeline = new DefaultSamplingPipeline { Temperature = 0.6f },
                MaxTokens = 200,
                AntiPrompts = new List<string> { "<|eot_id|>" }
            };

            var prompts = new[]
            {
                "Extract ONLY this person's FULL NAME. If not clearly stated, output nothing at all.\n\nText: \"Hey everyone, I'm Sarah Mitchell, and today we're taking a tour of my studio where I run CraftCo.\"\n",
                "Extract ONLY this person's COMPANY. If not clearly stated, output nothing at all.\n\nText: \"Hi, I'm Sarah Mitchell, founder of CraftCo.\"\n",
                "You are a copywriter. Write ONE personalized first-line icebreaker for a YouTube creator named Alex who makes videos about film editing. 1 sentence, plain text, no greeting.\n"
            };

            Console.WriteLine($"\nRunning {prompts.Length} inference(s)...\n");

            foreach (var (prompt, i) in prompts.Select((p, i) => (p, i)))
            {
                var run = Stopwatch.StartNew();
                var builder = new System.Text.StringBuilder();
                int tokens = 0;
                await foreach (var token in executor.InferAsync(prompt, inferenceParams))
                {
                    builder.Append(token);
                    tokens++;
                }
                run.Stop();

                Console.WriteLine($"---- Prompt #{i + 1} ({run.Elapsed.TotalSeconds:N1}s, {tokens} tokens) ----");
                Console.WriteLine(builder.ToString().Trim());
                Console.WriteLine();
            }

            Console.WriteLine("SMOKE TEST COMPLETE");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: an error occurred.\n{ex.Message}\n{ex}");
            return 1;
        }
    }

    private static string? ResolveModelPath(string[] args)
    {
        var idx = Array.IndexOf(args, "--model");
        if (idx >= 0 && idx + 1 < args.Length)
            return Path.GetFullPath(args[idx + 1]);

        // Try a few reasonable locations relative to the working directory / output.
        var cwd = Directory.GetCurrentDirectory();
        var candidates = new[]
        {
            Path.Combine(cwd, "TubeMailGorilla.Maui", "Resources", "Models", "Llama-3.2-3B-Instruct-Q4_K_M.gguf"),
            Path.GetFullPath(Path.Combine(cwd, "..", "TubeMailGorilla.Maui", "Resources", "Models", "Llama-3.2-3B-Instruct-Q4_K_M.gguf")),
            Path.Combine(cwd, "Llama-3.2-3B-Instruct-Q4_K_M.gguf")
        };
        foreach (var full in candidates)
        {
            if (File.Exists(full)) return full;
        }
        return null;
    }
}