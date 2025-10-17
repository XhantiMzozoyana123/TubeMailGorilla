using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TubeMailGorilla.Constants;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Forms
{
    public partial class AI_Assistant : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // Chat history for token management
        private readonly List<(string Role, string Content)> _chatHistory = new();

        private const int ModelContextLimit = 4096;
        private const int MaxResponseTokens = 1000;

        public AI_Assistant()
        {
            InitializeComponent();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        private async void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("AI Assistance is now in procedure, please wait for it to complete...");

                if (cboFunctions.SelectedIndex == 0)
                {
                    var emailer = await _context.Emailers.ToListAsync();

                    foreach (var item in emailer)
                    {
                        var caption = await _context.Captions.FirstOrDefaultAsync(x => x.EmailerId == item.Id);

                        if (caption == null) continue;

                        var prompt = $"Find the real-name of this YouTuber by analyzing this transcript: {caption.Text}." + rtxtPrompt.Text;
                        var result = await GenerateTextAsync(prompt);

                        item.Author = result;
                        await _context.SaveChangesAsync();

                        lstResult.Items.Add($"Author for {item.Email}: {result}");
                    }

                    MessageBox.Show("Real names collected and saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (cboFunctions.SelectedIndex == 1)
                {
                    var emailer = await _context.Emailers.ToListAsync();

                    foreach (var item in emailer)
                    {
                        var icebreaker = await _context.Icebreakers.FirstOrDefaultAsync(x => x.EmailerId == item.Id);
                        var caption = await _context.Captions.FirstOrDefaultAsync(x => x.EmailerId == item.Id);

                        if (caption == null || icebreaker == null) continue;

                        var prompt = $"Generate a professional and engaging icebreaker for an email to {item.Author}, based on this video transcript: {caption.Text}." + rtxtPrompt.Text;
                        var result = await GenerateTextAsync(prompt);

                        icebreaker.Text = result;
                        await _context.SaveChangesAsync();

                        lstResult.Items.Add($"Icebreaker for {item.Author}: {result}");
                    }

                    MessageBox.Show("Icebreakers generated and saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (cboFunctions.SelectedIndex == 2)
                {
                    var emailer = await _context.Emailers.ToListAsync();

                    foreach (var item in emailer)
                    {
                        var prompt = $"Fix this email address format: {item.Email}." + rtxtPrompt.Text;
                        var result = await GenerateTextAsync(prompt);

                        item.Email = result;
                        await _context.SaveChangesAsync();

                        lstResult.Items.Add($"Fixed Email: {result}");
                    }

                    MessageBox.Show("Email addresses fixed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AI_Assistant_Load(object sender, EventArgs e)
        {
            cboFunctions.Items.Add("Get Authors real name.");
            cboFunctions.Items.Add("Generate Icebreakers for email body.");
            cboFunctions.Items.Add("Fix email address formats.");

            cboFunctions.SelectedIndex = 0;
        }

        private async Task<string> GenerateTextAsync(string prompt)
        {
            try
            {
                // Add user prompt to chat history
                _chatHistory.Add(("user", prompt));

                // Trim history if needed
                TrimChatHistory();

                var apiKey = _context.Settings.FirstOrDefault()?.GoogleAiApiKey;
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: Missing Google AI API Key.";

                string model = "gemini-2.0-flash";
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

                // Build contents payload
                var contents = _chatHistory.Select(msg => new
                {
                    parts = new[] { new { text = msg.Content } }
                }).ToList();

                var payload = new { contents };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(payload, options);
                using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                // Add API key in header
                _httpClient.DefaultRequestHeaders.Remove("X-goog-api-key");
                _httpClient.DefaultRequestHeaders.Add("X-goog-api-key", apiKey);

                var response = await _httpClient.PostAsync(url, stringContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {errorContent}";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                // Safely extract text
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textProp))
                {
                    var text = textProp.GetString();
                    if (!string.IsNullOrEmpty(text))
                        _chatHistory.Add(("assistant", text));
                    return text ?? "No response";
                }

                return "No response found in API output";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        private void TrimChatHistory()
        {
            int totalTokens = 0;

            for (int i = _chatHistory.Count - 1; i >= 0; i--)
            {
                int messageTokens = _chatHistory[i].Content.Length / 4;
                totalTokens += messageTokens;

                if (totalTokens > ModelContextLimit - MaxResponseTokens)
                {
                    _chatHistory.RemoveRange(0, i);
                    break;
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstResult.Items.Clear();
        }
    }
}
