using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using TubeMailGorillaApplication.Interfaces;

namespace TubeMailGorillaInfrastructure.External
{
    public class LocalAIService : IAIService, IDisposable
    {
        private readonly string _modelPath;
        private readonly ILogger<LocalAIService>? _logger;
        private LLamaContext? _context;
        private InteractiveExecutor? _executor;
        private readonly object _lock = new();

        private const int DefaultMaxTokens = 512;
        private const float DefaultTemperature = 0.6f;

        public LocalAIService(string modelPath, ILogger<LocalAIService>? logger = null)
        {
            _modelPath = modelPath;
            _logger = logger;
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            try
            {
                EnsureModelLoaded();

                var inferenceParams = new InferenceParams
                {
                    MaxTokens = DefaultMaxTokens,
                    AntiPrompts = new List<string> { "User:", "\n\nHuman:" }
                };

                var result = new System.Text.StringBuilder();

                // Run inference synchronously then wrap in Task
                await foreach (var text in _executor!.InferAsync(prompt, inferenceParams))
                {
                    result.Append(text);
                }

                var response = result.ToString().Trim();

                if (string.IsNullOrEmpty(response))
                    return "No response generated.";

                return response;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "LLM inference failed for prompt (first 50 chars): {Prompt}", 
                    prompt.Length > 50 ? prompt[..50] : prompt);
                return $"LLM Error: {ex.Message}";
            }
        }

        private void EnsureModelLoaded()
        {
            if (_context != null && _executor != null)
                return;

            lock (_lock)
            {
                if (_context != null && _executor != null)
                    return;

                if (!File.Exists(_modelPath))
                {
                    throw new FileNotFoundException(
                        $"LLM model file not found at: {_modelPath}\n" +
                        "Please download a Llama 3 GGUF model (e.g., Llama-3.2-1B-Instruct-Q4_K_M.gguf or Llama-3.2-3B-Instruct-Q4_K_M.gguf) " +
                        "from Hugging Face and place it at the configured path.");
                }

                var parameters = new ModelParams(_modelPath)
                {
                    ContextSize = 2048,
                    GpuLayerCount = 0, // CPU-only; set >0 for GPU acceleration
                    UseMemoryLock = false,
                    Threads = Environment.ProcessorCount / 2
                };

                _logger?.LogInformation("Loading LLM model from {ModelPath}...", _modelPath);

                using var weights = LLamaWeights.LoadFromFile(parameters);
                _context = weights.CreateContext(parameters);
                _executor = new InteractiveExecutor(_context);

                _logger?.LogInformation("Model loaded successfully.");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
            _executor = null;
            _context = null;
        }
    }
}
