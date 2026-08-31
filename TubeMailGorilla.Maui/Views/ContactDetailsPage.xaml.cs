using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class ContactDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private bool _isGeneratingIcebreaker;

    public ContactDetailsPage(EmailContact contact)
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _ai = ServiceHelper.GetService<AIService>();
        BindingContext = contact;
        NameEntry.Text = contact.Name ?? string.Empty;
        EmailEntry.Text = contact.Email;
        _ = LoadOpenersAsync();
    }

    /// <summary>
    /// Loads previously generated icebreakers for this lead so they can be
    /// reviewed (and regenerated) while editing the contact.
    /// </summary>
    private async Task LoadOpenersAsync()
    {
        if (BindingContext is not EmailContact contact || contact.Id == 0)
        {
            NoOpenersLabel.IsVisible = true;
            OpenersList.IsVisible = false;
            return;
        }

        try
        {
            var openers = await _db.GetOpenersForLeadAsync(contact.Id);
            OpenersList.ItemsSource = openers;
            OpenersList.IsVisible = openers.Count > 0;
            NoOpenersLabel.IsVisible = openers.Count == 0;
        }
        catch
        {
            NoOpenersLabel.IsVisible = true;
            OpenersList.IsVisible = false;
        }
    }

    /// <summary>
    /// Generates a fresh AI icebreaker for this lead and saves it as an
    /// Opener. If the contact hasn't been saved yet, it is saved first so
    /// the opener has a lead Id to attach to.
    /// </summary>
    private async void OnGenerateIcebreakerClicked(object? sender, EventArgs e)
    {
        if (_isGeneratingIcebreaker) return;
        if (BindingContext is not EmailContact contact) return;

        // A new contact must exist in the DB before we can attach an opener.
        if (contact.Id == 0)
        {
            contact.Name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text.Trim();
            contact.Email = (EmailEntry.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(contact.Email))
            {
                await DisplayAlert("Email required", "Please enter an email address before generating an icebreaker.", "OK");
                return;
            }
            contact.ExtractedAt = DateTime.Now;
            await _db.AddContactAsync(contact);
        }

        _isGeneratingIcebreaker = true;
        GenerateIcebreakerButton.IsEnabled = false;
        GenerateIcebreakerButton.Text = "✨ Writing…";

        try
        {
            var icebreaker = await _ai.GenerateIcebreakerAsync(contact);

            if (string.IsNullOrWhiteSpace(icebreaker))
            {
                await DisplayAlert("Generation failed",
                    "Could not generate an icebreaker. Check your internet connection / API key and try again.", "OK");
                return;
            }

            await _db.SaveOpenerAsync(new Opener
            {
                EmailerId = contact.Id,
                Text = icebreaker,
                CreatedAt = DateTime.Now
            });

            await LoadOpenersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not generate icebreaker: {ex.Message}", "OK");
        }
        finally
        {
            _isGeneratingIcebreaker = false;
            GenerateIcebreakerButton.IsEnabled = true;
            GenerateIcebreakerButton.Text = "✨ Generate Icebreaker";
        }
    }

    private async void OnDeleteOpenerClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.BindingContext is not Opener opener) return;

        try
        {
            await _db.DeleteOpenerAsync(opener.Id);
            await LoadOpenersAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete icebreaker: {ex.Message}", "OK");
        }
    }

        private async void OnSave(object? sender, EventArgs e)
    {
        if (BindingContext is not EmailContact contact) return;

        contact.Name = string.IsNullOrWhiteSpace(NameEntry.Text) ? null : NameEntry.Text.Trim();
        contact.Email = (EmailEntry.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(contact.Email))
        {
            await DisplayAlert("Email required", "Please enter an email address.", "OK");
            return;
        }

        if (contact.Id == 0)
            await _db.AddContactAsync(contact);
        else
        {
            contact.UpdatedAt = DateTime.Now;
            await _db.UpdateContactAsync(contact);
        }
        await Navigation.PopAsync();
    }

        private async void OnDelete(object? sender, EventArgs e)
    {
        if (BindingContext is not EmailContact contact) return;

        var confirm = await DisplayAlert(
            "Delete contact?",
            $"Remove \"{contact.Email}\" from your contacts?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        await _db.DeleteContactAsync(contact.Id);
        await Navigation.PopAsync();
    }
}