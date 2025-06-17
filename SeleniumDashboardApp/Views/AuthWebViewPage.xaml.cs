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
    private string _lastUrl = "";

    public AuthWebViewPage(string authUrl, string redirectUri, TaskCompletionSource<AuthResult> tcs)
    {
        InitializeComponent();
        _authUrl = authUrl;
        _redirectUri = redirectUri;
        _authTaskCompletionSource = tcs;

        System.Diagnostics.Debug.WriteLine($"=== AUTH WEBVIEW PAGE ===");
        System.Diagnostics.Debug.WriteLine($"Auth URL: {_authUrl}");
        System.Diagnostics.Debug.WriteLine($"Redirect URI: {_redirectUri}");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("=== AUTH WEBVIEW: Loading Auth0 URL ===");

        // Load the auth URL
        AuthWebView.Source = _authUrl;
        AuthWebView.IsVisible = true;
        LoadingStack.IsVisible = false;

        // Start a simple timer that checks URL changes
        Device.StartTimer(TimeSpan.FromSeconds(2), () =>
        {
            if (_authCompleted) return false;

            try
            {
                var currentUrl = AuthWebView.Source?.ToString() ?? "";

                // Check if URL changed from login page
                if (!string.IsNullOrEmpty(currentUrl) && currentUrl != _lastUrl)
                {
                    System.Diagnostics.Debug.WriteLine($"URL changed: {currentUrl}");
                    _lastUrl = currentUrl;

                    // If we're no longer on the auth0 login page, try to get auth code
                    if (!currentUrl.Contains("authorize") && currentUrl.Contains("auth0.com"))
                    {
                        System.Diagnostics.Debug.WriteLine("Detected redirect after login - trying to extract auth code");
                        _ = Task.Run(async () => await TryExtractAuthCode());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Timer error: {ex.Message}");
            }

            return !_authCompleted; // Continue until auth completed
        });
    }

    private async Task TryExtractAuthCode()
    {
        if (_authCompleted) return;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    // Try to get the current URL and check for auth code
                    var currentUrl = AuthWebView.Source?.ToString() ?? "";
                    System.Diagnostics.Debug.WriteLine($"Trying to extract auth code from: {currentUrl}");

                    // Check if URL contains auth code
                    if (currentUrl.Contains("code="))
                    {
                        await HandleUrlWithCode(currentUrl);
                        return;
                    }

                    // Try JavaScript to get the actual current URL (in case WebView.Source is cached)
                    var actualUrl = await AuthWebView.EvaluateJavaScriptAsync("window.location.href");
                    System.Diagnostics.Debug.WriteLine($"Actual URL from JavaScript: {actualUrl}");

                    if (!string.IsNullOrEmpty(actualUrl) && actualUrl.Contains("code="))
                    {
                        await HandleUrlWithCode(actualUrl);
                        return;
                    }

                    // Try to check if we can find any hidden form fields or elements with auth code
                    var authCodeScript = @"
                        // Look for auth code in various places
                        var url = window.location.href;
                        var urlParams = new URLSearchParams(window.location.search);
                        var hashParams = new URLSearchParams(window.location.hash.substring(1));
                        
                        var code = urlParams.get('code') || hashParams.get('code');
                        if (code) return code;
                        
                        // Check for hidden inputs
                        var hiddenInputs = document.querySelectorAll('input[type=""hidden""]');
                        for (var input of hiddenInputs) {
                            if (input.name.includes('code') || input.id.includes('code')) {
                                return input.value;
                            }
                        }
                        
                        // Check for any element containing what looks like an auth code
                        var bodyText = document.body.innerText;
                        var codeMatch = bodyText.match(/[a-zA-Z0-9_-]{20,}/);
                        if (codeMatch) return codeMatch[0];
                        
                        return null;
                    ";

                    var jsResult = await AuthWebView.EvaluateJavaScriptAsync(authCodeScript);
                    System.Diagnostics.Debug.WriteLine($"JavaScript auth code search result: {jsResult}");

                    if (!string.IsNullOrEmpty(jsResult) && jsResult != "null" && jsResult.Length > 10)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found potential auth code: {jsResult}");
                        await HandleAuthCode(jsResult);
                        return;
                    }

                    // If we're here and not on a login page anymore, something might be wrong
                    if (!currentUrl.Contains("login") && currentUrl.Contains("auth0.com"))
                    {
                        System.Diagnostics.Debug.WriteLine("Seems like login completed but no auth code found");

                        // Wait a bit more and try one more time
                        await Task.Delay(3000);

                        if (!_authCompleted)
                        {
                            var finalUrl = await AuthWebView.EvaluateJavaScriptAsync("window.location.href");
                            System.Diagnostics.Debug.WriteLine($"Final URL check: {finalUrl}");

                            if (finalUrl.Contains("code="))
                            {
                                await HandleUrlWithCode(finalUrl);
                            }
                            else
                            {
                                // Last resort: manually redirect to callback
                                System.Diagnostics.Debug.WriteLine("Manually redirecting to callback URL");
                                AuthWebView.Source = _redirectUri + "?manual_redirect=true";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in TryExtractAuthCode: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in TryExtractAuthCode main: {ex.Message}");
        }
    }

    private async Task HandleUrlWithCode(string url)
    {
        if (_authCompleted) return;

        try
        {
            var uri = new Uri(url);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var fragment = uri.Fragment?.TrimStart('#');
            var fragmentParams = string.IsNullOrEmpty(fragment) ?
                new System.Collections.Specialized.NameValueCollection() :
                System.Web.HttpUtility.ParseQueryString(fragment);

            var authCode = queryParams["code"] ?? fragmentParams["code"];
            var error = queryParams["error"] ?? fragmentParams["error"];

            if (!string.IsNullOrEmpty(error))
            {
                await HandleAuthError(error);
            }
            else if (!string.IsNullOrEmpty(authCode))
            {
                await HandleAuthCode(authCode);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing URL: {ex.Message}");
        }
    }

    private async Task HandleAuthCode(string authCode)
    {
        if (_authCompleted) return;

        _authCompleted = true;
        System.Diagnostics.Debug.WriteLine($"Auth code found: {authCode.Substring(0, Math.Min(10, authCode.Length))}...");

        _authTaskCompletionSource.SetResult(new AuthResult
        {
            IsSuccess = true,
            AuthorizationCode = authCode,
            State = ""
        });

        await Navigation.PopModalAsync();
    }

    private async Task HandleAuthError(string error)
    {
        if (_authCompleted) return;

        _authCompleted = true;
        System.Diagnostics.Debug.WriteLine($"Auth error: {error}");

        _authTaskCompletionSource.SetResult(new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = error
        });

        await Navigation.PopModalAsync();
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"=== NAVIGATING TO: {e.Url} ===");

        // Check for callback URL immediately
        if (e.Url.Contains("code=") || e.Url.Contains("error="))
        {
            System.Diagnostics.Debug.WriteLine("Found callback URL during navigation!");
            _ = Task.Run(async () => await HandleUrlWithCode(e.Url));
        }
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"=== NAVIGATED TO: {e.Url} ===");

        // Check for callback URL after navigation
        if (e.Url.Contains("code=") || e.Url.Contains("error="))
        {
            System.Diagnostics.Debug.WriteLine("Found callback URL after navigation!");
            _ = Task.Run(async () => await HandleUrlWithCode(e.Url));
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (_authCompleted) return;

        System.Diagnostics.Debug.WriteLine("=== AUTH WEBVIEW: User cancelled ===");

        _authCompleted = true;
        _authTaskCompletionSource.SetResult(new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = "Authentication cancelled by user"
        });

        await Navigation.PopModalAsync();
    }
}