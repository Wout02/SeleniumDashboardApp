using Microsoft.Maui.Controls;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.Views.Shared;

public partial class TopBarView : ContentView
{
    private readonly AuthService _authService;

    public static readonly BindableProperty ShowBackButtonProperty =
        BindableProperty.Create(nameof(ShowBackButton), typeof(bool), typeof(TopBarView), false, propertyChanged: OnShowBackButtonChanged);

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    private static void OnShowBackButtonChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TopBarView topBar && topBar.BackButton != null)
        {
            topBar.BackButton.IsVisible = (bool)newValue;
        }
    }

    public TopBarView()
    {
        InitializeComponent();
        _authService = new AuthService();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync(true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating back: {ex.Message}");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== STARTING LOGOUT ===");

            LoadingIndicator.IsVisible = true;
            LogoutButton.IsEnabled = false;
            LogoutButton.Text = "Uitloggen...";

            await _authService.LogoutAsync();

            System.Diagnostics.Debug.WriteLine("Logout completed, triggering new login...");

            LoadingIndicator.IsVisible = false;
            LogoutButton.IsEnabled = true;
            LogoutButton.Text = "Uitloggen";

            var token = await _authService.LoginAsync();

            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set("access_token", token);
                System.Diagnostics.Debug.WriteLine("New login successful after logout");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("User cancelled login after logout");

                await Application.Current.MainPage.DisplayAlert(
                    "Uitgelogd",
                    "Je bent uitgelogd. Herstart de app om opnieuw in te loggen.",
                    "OK");

                Application.Current.Quit();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");

            LoadingIndicator.IsVisible = false;
            LogoutButton.IsEnabled = true;
            LogoutButton.Text = "Uitloggen";

            await Application.Current.MainPage.DisplayAlert(
                "Fout bij uitloggen",
                "Er ging iets mis. Herstart de app om opnieuw in te loggen.",
                "OK");
        }
    }
}