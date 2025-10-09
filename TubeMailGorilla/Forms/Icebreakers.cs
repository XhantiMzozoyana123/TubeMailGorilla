using Microsoft.EntityFrameworkCore;
using System;
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
    public partial class Icebreakers : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public Icebreakers()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstResult.Items.Clear();
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedEmailer = await _context.Emailers
                    .FirstOrDefaultAsync(x => x.Email == cboEmail.Text);

                if (selectedEmailer == null)
                {
                    MessageBox.Show("Selected emailer not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var icebreaker = await _context.Icebreakers
                    .FirstOrDefaultAsync(x => x.EmailerId == selectedEmailer.Id);

                var caption = await _context.Captions
                    .FirstOrDefaultAsync(x => x.EmailerId == selectedEmailer.Id);

                if (caption == null)
                {
                    MessageBox.Show("No caption found for this emailer.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var prompt = $"Generate me a first-line for my email that complements this youtuber based on what they are saying here: {caption.Text} " + rtxtMessage.Text;
                var result = await GenerateTextAsync(prompt);

                if (icebreaker == null)
                {
                    // Create a new Icebreaker if not found
                    icebreaker = new Icebreaker
                    {
                        EmailerId = selectedEmailer.Id,
                        Text = result,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Icebreakers.Add(icebreaker);
                }
                else
                {
                    // Update existing
                    icebreaker.Text = result;
                    icebreaker.CreatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                lstResult.Items.Add($"✅ Icebreaker saved for {selectedEmailer.Email} at {icebreaker.CreatedAt}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add icebreaker:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Icebreakers_Load(object sender, EventArgs e)
        {
            var query = _context.Emailers.ToList();
            foreach (var item in query)
            {
                cboEmail.Items.Add(item.Email); // Use email string instead of object
            }

            if (cboEmail.Items.Count > 0)
                cboEmail.SelectedIndex = 0;
        }

        private async Task<string> GenerateTextAsync(string prompt)
        {
            await Task.Delay(8000); // To avoid rate limiting

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var apiKey = _context.Settings.FirstOrDefault()?.GoogleAiApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
                return "Error: Missing Google AI API Key.";

            var response = await _httpClient.PostAsync(
                AppConstant.googleAiEndPoint + apiKey,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode}";

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response";
        }

        private void cboEmail_SelectedIndexChanged(object sender, EventArgs e)
        {
            var emailer = _context.Emailers.FirstOrDefault(x => x.Email == cboEmail.Text);
            var icebreaker = _context.Icebreakers.FirstOrDefault(x => x.EmailerId == emailer.Id);

            rtxtMessage.Text = icebreaker.Text;
        }
    }
}
