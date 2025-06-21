using Microsoft.Maui.Controls;
using SeleniumDashboardApp.Services;
using System;
using System.Threading.Tasks;

namespace SeleniumDashboardApp.Views;

public partial class AuthWebViewPage : ContentPage
{
    private readonly string _authUrl;
    private readonly string _redirectUri;
    private readonly TaskCompletionSource<AuthResult> _authTaskCompletionSource;
    private bool _authCompleted = false;

    public AuthWebViewPage(string authUrl, string redirectUri, TaskCompletionSource<AuthResult> tcs)
    {
        InitializeComponent();
        _authUrl = authUrl;
        _redirectUri = redirectUri;
        _authTaskCompletionSource = tcs;

        System.Diagnostics.Debug.WriteLine($"=== AUTH WEBVIEW CREATED ===");
        System.Diagnostics.Debug.WriteLine($"Auth URL: {_authUrl}");
        System.Diagnostics.Debug.WriteLine($"Redirect URI: {_redirectUri}");

        // Set title to show we're loading
        Title = "Loading Auth...";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("=== AUTH WEBVIEW: OnAppearing ===");

        try
        {
            // Load immediately
            System.Diagnostics.Debug.WriteLine("Setting WebView source...");
            AuthWebView.Source = _authUrl;
            AuthWebView.IsVisible = true;
            LoadingStack.IsVisible = false;
            Title = "Auth0 Login";

            // Set a timeout
            Device.StartTimer(TimeSpan.FromSeconds(60), () =>
            {
                if (!_authCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("=== TIMEOUT: Authentication took too long ===");
                    CompleteWithError("Authentication timeout - please try again");
                }
                return false;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR IN OnAppearing: {ex.Message} ===");
            CompleteWithError($"Failed to start auth: {ex.Message}");
        }
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"=== NAVIGATING: {e.Url} ===");

        // Update title to show current action
        Title = "Authenticating...";

        if (e.Url.StartsWith("mauiapp://callback"))
        {
            System.Diagnostics.Debug.WriteLine("=== CALLBACK DETECTED! ===");
            e.Cancel = true;
            HandleCallback(e.Url);
        }
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"=== NAVIGATED: {e.Url} ===");

        if (e.Url.StartsWith("mauiapp://callback"))
        {
            System.Diagnostics.Debug.WriteLine("=== CALLBACK DETECTED AFTER NAV! ===");
            HandleCallback(e.Url);
        }
        else if (e.Url.Contains("login") || e.Url.Contains("auth0.com"))
        {
            Title = "Please login";
        }
    }

    private void HandleCallback(string url)
    {
        if (_authCompleted) return;

        System.Diagnostics.Debug.WriteLine($"=== PROCESSING CALLBACK: {url} ===");

        try
        {
            var uri = new Uri(url);
            var query = uri.Query;

            System.Diagnostics.Debug.WriteLine($"Query string: {query}");

            if (string.IsNullOrEmpty(query))
            {
                CompleteWithError("No query parameters in callback");
                return;
            }

            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            var code = queryParams["code"];
            var error = queryParams["error"];

            System.Diagnostics.Debug.WriteLine($"Error: {error}");

            if (!string.IsNullOrEmpty(error))
            {
                CompleteWithError($"Auth0 error: {error}");
                return;
            }

            if (string.IsNullOrEmpty(code))
            {
                CompleteWithError("No authorization code received");
                return;
            }

            CompleteWithSuccess(code);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== CALLBACK ERROR: {ex.Message} ===");
            CompleteWithError($"Callback parsing error: {ex.Message}");
        }
    }

    private void CompleteWithSuccess(string code)
    {
        if (_authCompleted) return;
        _authCompleted = true;

        System.Diagnostics.Debug.WriteLine($"=== AUTH SUCCESS: {code.Substring(0, 10)}... ===");
        Title = "Login successful!";

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _authTaskCompletionSource.SetResult(new AuthResult
            {
                IsSuccess = true,
                AuthorizationCode = code
            });

            await Navigation.PopModalAsync();
        });
    }

    private void CompleteWithError(string error)
    {
        if (_authCompleted) return;
        _authCompleted = true;

        System.Diagnostics.Debug.WriteLine($"=== AUTH ERROR: {error} ===");
        Title = "Login failed";

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _authTaskCompletionSource.SetResult(new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = error
            });

            await Navigation.PopModalAsync();
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== USER CANCELLED ===");
        CompleteWithError("User cancelled authentication");
    }
}