using CommunityToolkit.Mvvm.ComponentModel;

namespace TubeMailGorilla.Maui.Unlocked.ViewModels;

/// <summary>
/// Base view model providing shared observable state for all view models.
/// The unlocked edition has no sign-in state, so this only carries the
/// "busy / title" plumbing a view model needs.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;
}