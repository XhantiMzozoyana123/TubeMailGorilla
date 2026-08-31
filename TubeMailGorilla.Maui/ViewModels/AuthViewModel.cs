using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui.ViewModels
{
    public partial class AuthViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string? _fullName = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// When <c>false</c> the page presents the Login flow; when <c>true</c>
        /// the registration fields and Register action are shown instead.
        /// </summary>
        [ObservableProperty]
        private bool _isRegisterMode = false;

        public AuthViewModel(AuthService authService)
        {
            _authService = authService;
            IsLoggedIn = !string.IsNullOrEmpty(Preferences.Get("AuthToken", string.Empty));
        }

        /// <summary>
        /// Safe alert helper. The auth screen can run outside a Shell (it is
        /// hosted in a plain NavigationPage on first launch), so
        /// <see cref="Shell.Current"/> may be <c>null</c> here — resolve the
        /// active page from the application windows instead.
        /// </summary>
        private static Task DisplayAlertSafeAsync(string title, string message, string cancel)
        {
            var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
            return page is null ? Task.CompletedTask : page.DisplayAlertAsync(title, message, cancel);
        }

        /// <summary>
        /// Swaps the visible root page of the main window. Setting
        /// <c>Window.Page</c> directly is used instead of Shell routes because
        /// the auth screen lives outside any Shell and no Shell routes exist.
        /// </summary>
        private static void SwapRootPage(Page newPage)
        {
            // Delegates to the app-level window replacement, which opens a
            // fresh window (reliable on Windows) instead of mutating the
            // existing one's Page property.
            App.ReplaceRoot(newPage);
        }

        /// <summary>
        /// Toggles between Login and Register modes. Pass <c>"register"</c> to
        /// switch into registration mode, anything else switches back to Login.
        /// Wired to the Sign Up / Sign In links on the auth page.
        /// </summary>
        [RelayCommand]
        private void SwitchMode(string mode)
        {
            IsRegisterMode = mode?.Equals("register", StringComparison.OrdinalIgnoreCase) == true;
        }

        [RelayCommand]
        public async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await DisplayAlertSafeAsync("Error", "Email and password are required", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _authService.RegisterAsync(Email, Password, FullName);
                if (result.Success)
                {
                    await DisplayAlertSafeAsync("Success", "Registration successful! Please login.", "OK");
                    await LoginAsync();
                }
                else
                {
                    await DisplayAlertSafeAsync("Error", result.Message ?? "Registration failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertSafeAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await DisplayAlertSafeAsync("Error", "Email and password are required", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var result = await _authService.LoginAsync(Email, Password);
                if (result.Success)
                {
                    // Persist token and swap the main page to the app shell.
                    // NOTE: Shell route navigation (GoToAsync) is NOT used here
                    // because on first launch AuthView is hosted in a plain
                    // NavigationPage (Shell.Current is null) and no Shell
                    // routes were ever registered.
                    Preferences.Set("AuthToken", result.Data);
                    IsLoggedIn = true;

                    var shell = new AppShell();
                    SwapRootPage(shell);
                }
                else
                {
                    await DisplayAlertSafeAsync("Error", result.Message ?? "Login failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertSafeAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GetCurrentUserAsync()
        {
            var result = await _authService.GetCurrentUserAsync();
            if (result.Success)
            {
                FullName = result.Data?.FullName ?? string.Empty;
                Email = result.Data?.Email ?? string.Empty;
                await DisplayAlertSafeAsync("User Info", $"Logged in as: {result.Data?.Email}", "OK");
            }
            else
            {
                await DisplayAlertSafeAsync("Error", result.Message ?? "Failed to get user info", "OK");
            }
        }

        [RelayCommand]
        public async Task LogoutAsync()
        {
            var result = await _authService.LogoutAsync();
            if (result.Success)
            {
                // Clear local state
                Email = string.Empty;
                Password = string.Empty;
                FullName = string.Empty;
                IsLoggedIn = false;
                IsRegisterMode = false;

                // Swap back to a fresh auth screen. A brand-new AuthView is
                // created so its BindingContext (and thus IsLoggedIn) is
                // rebuilt from the now-empty Preferences.
                SwapRootPage(new NavigationPage(new Views.AuthView()));
            }
        }
    }
}