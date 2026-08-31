using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.Views;

public partial class EmailTemplateDetailsPage : ContentPage
{
    private readonly DatabaseService _db;
    private readonly EmailTemplate _template;

    public EmailTemplateDetailsPage(EmailTemplate? template = null, string? defaultSubject = null, string? defaultBody = null)
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
        _template = template ?? new EmailTemplate();

        NameEntry.Text = _template.Name;

        if (_template.Id == 0)
        {
            // Storing a new template, optionally pre-filled from the Send page composer.
            SubjectEntry.Text = defaultSubject ?? string.Empty;
            BodyEditor.Text = defaultBody ?? string.Empty;
        }
        else
        {
            SubjectEntry.Text = _template.Subject;
            BodyEditor.Text = _template.Body;
        }
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        _template.Name = (NameEntry.Text ?? string.Empty).Trim();
        _template.Subject = SubjectEntry.Text ?? string.Empty;
        _template.Body = BodyEditor.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_template.Name))
        {
            await DisplayAlert("Name required", "Please give this template a name.", "OK");
            return;
        }

        try
        {
            _template.UpdatedAt = DateTime.Now;
            await _db.SaveTemplateAsync(_template);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not save template: {ex.Message}", "OK");
        }
    }

    private async void OnCancel(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
