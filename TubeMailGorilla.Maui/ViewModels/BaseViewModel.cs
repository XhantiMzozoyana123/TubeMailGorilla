using CommunityToolkit.Mvvm.ComponentModel;

namespace TubeMailGorilla.Maui.ViewModels;

/// <summary>
/// Base view model providing shared observable state for all view models.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _canRegister = true;
}