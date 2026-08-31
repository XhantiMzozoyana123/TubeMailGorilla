using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TubeMailGorilla.Maui.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://api.tubemailgorilla.xyz";
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<Response<string>> RegisterAsync(string email, string password, string? fullName)
        {
            var model = new RegisterRequest(email, password, fullName);
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/auth/register", content);

            if (!response.IsSuccessStatusCode) return new Response<string> { Success = false, Message = "Registration failed" };

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponse>(jsonResponse, _jsonOptions);

            if (result == null || string.IsNullOrEmpty(result.Token)) return new Response<string> { Success = false, Message = "No token received" };

            Preferences.Set("AuthToken", result.Token);
            return new Response<string> { Success = true, Data = result.Token };
        }

        public async Task<Response<string>> LoginAsync(string email, string password)
        {
            var model = new LoginRequest(email, password);
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/auth/login", content);

            if (!response.IsSuccessStatusCode) return new Response<string> { Success = false, Message = "Login failed" };

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponse>(jsonResponse, _jsonOptions);

            if (result == null || string.IsNullOrEmpty(result.Token)) return new Response<string> { Success = false, Message = "No token received" };

            Preferences.Set("AuthToken", result.Token);
            return new Response<string> { Success = true, Data = result.Token };
        }

        public async Task<Response<UserInfo>> GetCurrentUserAsync()
        {
            var token = Preferences.Get("AuthToken", string.Empty);
            if (string.IsNullOrEmpty(token)) return new Response<UserInfo> { Success = false, Message = "No token found" };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/auth/user");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!response.IsSuccessStatusCode) return new Response<UserInfo> { Success = false, Message = "Failed to get user" };

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UserResponse>(jsonResponse, _jsonOptions);

            if (result == null) return new Response<UserInfo> { Success = false, Message = "Failed to parse response" };

            return new Response<UserInfo>
            {
                Success = true,
                Data = new UserInfo { Id = result.Id, Email = result.Email, FullName = result.FullName }
            };
        }

        public async Task<Response<bool>> LogoutAsync()
        {
            Preferences.Remove("AuthToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return new Response<bool> { Success = true, Data = true };
        }
    }

    public class AuthResponse { public string Token { get; set; } = string.Empty; public bool Success { get; set; } public string? Message { get; set; } }
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public RegisterRequest() { }
        public RegisterRequest(string email, string password, string? fullName) { Email = email; Password = password; FullName = fullName; }
    }
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public LoginRequest() { }
        public LoginRequest(string email, string password) { Email = email; Password = password; }
    }
    public class UserInfo { public string Id { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string? FullName { get; set; } }
    public class UserResponse { public string Id { get; set; } = string.Empty; public string Email { get; set; } = string.Empty; public string? FullName { get; set; } }

    public class Response<T> { public bool Success { get; set; } public string? Message { get; set; } public T? Data { get; set; } }
}