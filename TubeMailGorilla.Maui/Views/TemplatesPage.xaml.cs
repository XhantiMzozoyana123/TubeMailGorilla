using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TubeMailGorillaMaui.ViewModels;

namespace TubeMailGorilla.Maui.Views;

public partial class TemplatesPage : ContentPage
{
    private readonly TemplatesViewModel _viewModel;

    public TemplatesPage(TemplatesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        await _viewModel.LoadDataAsync();
        base.OnAppearing();
    }

    private async void OnSaveTemplateClicked(object sender, EventArgs e)
    {
        // Implementation would go here
        await DisplayAlert("Info", "Template saved successfully", "OK");
    }
}