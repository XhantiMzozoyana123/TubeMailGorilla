using TubeMailGorilla.Maui.Services;
using TubeMailGorilla.Maui.ViewModels;

namespace TubeMailGorilla.Maui.Views;

/// <summary>
/// Login/register page. The BindingContext is resolved from the application
/// service container (see ServiceHelper) because Shell expects a
/// parameterless constructor on pages.
/// </summary>
public partial class AuthView : ContentPage
{
    public AuthView()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<AuthViewModel>();
    }
}