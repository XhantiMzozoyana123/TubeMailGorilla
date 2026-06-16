using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    private string _extractType = "Max Result";
    public string ExtractType { get => _extractType; set => SetProperty(ref _extractType, value); }

    public IAsyncRelayCommand SaveCommand { get; }

    public SettingsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    private async Task SaveAsync()
    {
        var settings = await _unitOfWork.Settings.GetFirstAsync() ?? new Settings();
        settings.ExtractType = ExtractType;
        await _unitOfWork.CompleteAsync();
        await Shell.Current.DisplayAlert("Saved", "Settings saved.", "OK");
    }
}