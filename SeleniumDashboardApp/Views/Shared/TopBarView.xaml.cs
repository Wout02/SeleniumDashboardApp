using Microsoft.Maui.Controls;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.Views.Shared;

public partial class TopBarView : ContentView
{
    private readonly AuthService _authService;

    // Bindable property voor back button visibility
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
        _authService = new AuthService(); // Or use DI if you have it set up
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            // Navigate back to previous page
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

            // Show loading indicator and disable button
            LoadingIndicator.IsVisible = true;
            LogoutButton.IsEnabled = false;
            LogoutButton.Text = "Uitloggen...";

            // Perform simple logout (only clears local data)
            await _authService.LogoutAsync();

            System.Diagnostics.Debug.WriteLine("Logout completed, triggering new login...");

            // Reset UI first
            LoadingIndicator.IsVisible = false;
            LogoutButton.IsEnabled = true;
            LogoutButton.Text = "Uitloggen";

            // Now trigger a fresh login immediately with forced login prompt
            var token = await _authService.LoginAsync();

            if (!string.IsNullOrEmpty(token))
            {
                // Login successful - store token and stay on dashboard
                Preferences.Set("access_token", token);
                System.Diagnostics.Debug.WriteLine("New login successful after logout");
            }
            else
            {
                // Login failed or cancelled
                System.Diagnostics.Debug.WriteLine("User cancelled login after logout");

                await Application.Current.MainPage.DisplayAlert(
                    "Uitgelogd",
                    "Je bent uitgelogd. Herstart de app om opnieuw in te loggen.",
                    "OK");

                // Close the app since user doesn't want to login
                Application.Current.Quit();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");

            // Reset UI
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