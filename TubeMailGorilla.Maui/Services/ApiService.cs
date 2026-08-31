using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TubeMailGorilla.Maui.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://api.tubemailgorilla.xyz";
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}{endpoint}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Response<T>> PostAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}{endpoint}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Response<T>
                {
                    Success = false,
                    Message = $"API error: {response.StatusCode} - {errorContent}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new Response<T>
            {
                Success = true,
                Data = result
            };
        }

        public async Task<Response<T>> PutAsync<T>(string endpoint, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiBaseUrl}{endpoint}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Response<T>
                {
                    Success = false,
                    Message = $"API error: {response.StatusCode} - {errorContent}"
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new Response<T>
            {
                Success = true,
                Data = result
            };
        }

        public async Task<Response<bool>> DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}{endpoint}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Response<bool>
                {
                    Success = false,
                    Message = $"API error: {response.StatusCode} - {errorContent}"
                };
            }

            return new Response<bool>
            {
                Success = true,
                Data = true
            };
        }
        }
}