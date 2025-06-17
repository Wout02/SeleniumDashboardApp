using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace SeleniumDashboardApp.Views;

public partial class LogoutWebViewPage : ContentPage
{
    private readonly string _logoutUrl;
    private readonly TaskCompletionSource<bool> _logoutTaskCompletionSource;
    private bool _logoutCompleted = false;

    public LogoutWebViewPage(string logoutUrl, TaskCompletionSource<bool> tcs)
    {
        InitializeComponent();
        _logoutUrl = logoutUrl;
        _logoutTaskCompletionSource = tcs;

        System.Diagnostics.Debug.WriteLine($"=== LOGOUT WEBVIEW PAGE ===");
        System.Diagnostics.Debug.WriteLine($"Logout URL: {_logoutUrl}");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("=== LOGOUT WEBVIEW: Loading logout URL ===");

        // Small delay to show loading, then load logout URL
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
        {
            LogoutWebView.Source = _logoutUrl;
            LogoutWebView.IsVisible = true;
            LoadingStack.IsVisible = false;
            return false; // Don't repeat
        });

        // Auto-close after 8 seconds if logout doesn't complete
        Dispatcher.StartTimer(TimeSpan.FromSeconds(8), () =>
        {
            if (!_logoutCompleted)
            {
                System.Diagnostics.Debug.WriteLine("Logout timeout - auto completing");
                CompleteLogout();
            }
            return false;
        });
    }

    private async void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Logout WebView navigated to: {e.Url}");

        // Check if we've reached the logout completion URL
        if (e.Url.Contains("logout-complete") ||
            e.Url.Contains("seleniumdashboardapp") ||
            !e.Url.Contains("auth0.com"))
        {
            System.Diagnostics.Debug.WriteLine("Logout completed - detected completion URL");
            CompleteLogout();
        }
    }

    private async void CompleteLogout()
    {
        if (_logoutCompleted) return;

        _logoutCompleted = true;
        System.Diagnostics.Debug.WriteLine("=== LOGOUT COMPLETED ===");

        _logoutTaskCompletionSource.SetResult(true);
        await Navigation.PopModalAsync();
    }
}