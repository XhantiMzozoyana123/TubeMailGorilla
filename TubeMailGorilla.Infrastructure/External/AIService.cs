using System.Text;
using System.Text.Json;
using TubeMailGorillaApplication.Interfaces;
using static TubeMailGorillaApplication.Dtos.OllamaDto;

namespace TubeMailGorillaInfrastructure.External
{
    public class AIService : IAIService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public AIService()
        {
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            try
            {
                var request = new OllamaRequest
                {
                    Model = "llama3:latest",
                    Prompt = prompt,
                    Stream = false
                };

                var json = JsonSerializer.Serialize(request);

                var response = await _httpClient.PostAsync(
                    $"http://63.141.255.202/api/generate",
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"));

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<OllamaResponse>(
                    responseContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result?.Response ?? "No response returned.";
            }
            catch (Exception ex)
            {
                return $"LLM Error: {ex.Message}";
            }
        }
    }
}

