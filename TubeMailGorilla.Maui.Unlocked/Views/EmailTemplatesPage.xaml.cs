using System.Collections.Generic;
using System.Linq;
using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked.Views;

/// <summary>
/// Email template manager. The unlocked edition has no Pro gate: templates
/// are always fully available.
/// </summary>
public partial class EmailTemplatesPage : ContentPage
{
    private readonly DatabaseService _db;
    private List<EmailTemplate> _templates = new();

    public EmailTemplatesPage()
    {
        InitializeComponent();
        _db = ServiceHelper.GetService<DatabaseService>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            _templates = await _db.GetTemplatesAsync();
            TemplatesList.ItemsSource = _templates.OrderBy(t => t.Name).ToList();
            EmptyTemplatesLabel.IsVisible = _templates.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load templates: {ex.Message}", "OK");
            TemplatesList.ItemsSource = new List<EmailTemplate>();
            EmptyTemplatesLabel.IsVisible = true;
        }
    }

    private async void OnAddTemplateClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new EmailTemplateDetailsPage());
    }

    private async void OnTemplateSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not EmailTemplate template) return;
        TemplatesList.SelectedItem = null;
        await Navigation.PushAsync(new EmailTemplateDetailsPage(template));
    }

    private async void OnDeleteTemplateClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var template = button?.BindingContext as EmailTemplate;
        if (template == null) return;

        var confirm = await DisplayAlert(
            "Delete template?",
            $"Remove '{template.Name}' from your email templates?",
            "Delete",
            "Cancel");

        if (!confirm) return;

        try
        {
            await _db.DeleteTemplateAsync(template.Id);
            await LoadTemplatesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete template: {ex.Message}", "OK");
        }
    }
}
