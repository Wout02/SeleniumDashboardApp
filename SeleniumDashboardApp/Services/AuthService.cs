using Microsoft.Maui.Authentication;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeleniumDashboardApp.Services
{
    public class AuthService
    {
        private readonly string _domain = "dev-14kcc8n2c231d8b8.eu.auth0.com";
        private readonly string _clientId = "OG71NTgV1UMVeKjytuKivGyTmmkkOlOw";
        private readonly string _callbackUrl = "https://seleniumdashboardapp-production.up.railway.app/auth/callback";

        public async Task<string?> LoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== AUTH SERVICE: LoginAsync gestart ===");

#if WINDOWS
            return await WindowsWebViewLoginAsync();
#else
            return await MobileLoginAsync();
#endif
        }

#if WINDOWS
        private async Task<string?> WindowsWebViewLoginAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== WINDOWS: WebView Auth0 Login ===");
                
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);
                var state = GenerateState();

                var windowsCallbackUrl = "mauiapp://callback";

                var authUrl = $"https://{_domain}/authorize" +
                              $"?client_id={Uri.EscapeDataString(_clientId)}" +
                              $"&response_type=code" +
                              $"&scope={Uri.EscapeDataString("openid profile email")}" +
                              $"&redirect_uri={Uri.EscapeDataString(windowsCallbackUrl)}" +
                              $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                              $"&code_challenge_method=S256" +
                              $"&state={Uri.EscapeDataString(state)}" +
                              $"&prompt=login"; // This forces fresh login

                System.Diagnostics.Debug.WriteLine($"Windows WebView Auth URL: {authUrl}");

                var tcs = new TaskCompletionSource<AuthResult>();
                var authPage = new Views.AuthWebViewPage(authUrl, windowsCallbackUrl, tcs);
                
                await Application.Current.MainPage.Navigation.PushModalAsync(authPage);
                
                var authResult = await tcs.Task;
                
                if (!authResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Windows WebView auth failed: {authResult.ErrorMessage}");
                    return null;
                }
                
                var token = await ExchangeCodeForTokenAsync(authResult.AuthorizationCode, codeVerifier, windowsCallbackUrl);
                
                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("user_name", "Auth0 User (Windows)");
                    Preferences.Set("user_email", "user@auth0.com");
                    
                    System.Diagnostics.Debug.WriteLine("Windows WebView login successful!");
                    return token;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows WebView login error: {ex.Message}");
                return null;
            }
        }
#endif

        private async Task<string?> MobileLoginAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== MOBILE AUTH0 LOGIN: {DeviceInfo.Platform} ===");

                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);

                var authUrl = $"https://{_domain}/authorize" +
                              $"?client_id={Uri.EscapeDataString(_clientId)}" +
                              $"&response_type=code" +
                              $"&scope={Uri.EscapeDataString("openid profile email")}" +
                              $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
                              $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                              $"&code_challenge_method=S256" +
                              $"&prompt=login";

                var callbackScheme = "mauiapp://callback";

                var authRequest = new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(callbackScheme),
                    PrefersEphemeralWebBrowserSession = true // This helps with clean sessions
                };

                var result = await WebAuthenticator.Default.AuthenticateAsync(authRequest);

                if (result?.Properties == null)
                {
                    return null;
                }

                if (result.Properties.TryGetValue("error", out var error))
                {
                    System.Diagnostics.Debug.WriteLine($"Mobile auth error: {error}");
                    return null;
                }

                if (!result.Properties.TryGetValue("code", out var authorizationCode))
                {
                    return null;
                }

                var token = await ExchangeCodeForTokenAsync(authorizationCode, codeVerifier);

                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("user_name", "Auth0 User (Mobile)");
                    Preferences.Set("user_email", "user@auth0.com");
                }

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Mobile login error: {ex.Message}");
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== LOGOUT: Starting logout process ===");

            try
            {
                // 1. Clear API service authentication first
                try
                {
                    var serviceProvider = IPlatformApplication.Current?.Services;
                    var apiService = serviceProvider?.GetService<ApiService>();
                    if (apiService != null)
                    {
                        apiService.SetToken(""); // Clear the token
                        System.Diagnostics.Debug.WriteLine("API Service token cleared");
                    }
                }
                catch (Exception apiEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing API service: {apiEx.Message}");
                }

                // 2. Clear local storage
                Preferences.Remove("access_token");
                Preferences.Remove("user_name");
                Preferences.Remove("user_email");

                // 3. Set logout flag
                Preferences.Set("user_logged_out", "true");

                System.Diagnostics.Debug.WriteLine("Local storage cleared and logout flag set");

                // 4. Small delay to ensure cleanup
                await Task.Delay(100);

                System.Diagnostics.Debug.WriteLine("Logout completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

#if WINDOWS
        private async Task ClearWebViewSessionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CLEARING WEBVIEW SESSION ===");

                // Create logout URL
                var logoutUrl = $"https://{_domain}/v2/logout" +
                               $"?client_id={Uri.EscapeDataString(_clientId)}" +
                               $"&returnTo={Uri.EscapeDataString("mauiapp://logout-complete")}";

                System.Diagnostics.Debug.WriteLine($"Logout URL: {logoutUrl}");

                // Show logout in WebView to clear Auth0 session
                var tcs = new TaskCompletionSource<bool>();
                var logoutPage = new Views.LogoutWebViewPage(logoutUrl, tcs);
                
                await Application.Current.MainPage.Navigation.PushModalAsync(logoutPage);
                await tcs.Task; // Wait for logout to complete

                System.Diagnostics.Debug.WriteLine("WebView session cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing WebView session: {ex.Message}");
            }
        }
#endif

        private async Task ClearMobileSessionAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CLEARING MOBILE SESSION ===");

                // For mobile, use WebAuthenticator to clear session
                var logoutUrl = $"https://{_domain}/v2/logout" +
                               $"?client_id={Uri.EscapeDataString(_clientId)}" +
                               $"&returnTo={Uri.EscapeDataString(_callbackUrl)}";

                var logoutRequest = new WebAuthenticatorOptions
                {
                    Url = new Uri(logoutUrl),
                    CallbackUrl = new Uri(_callbackUrl),
                    PrefersEphemeralWebBrowserSession = true
                };

                // This will open browser, logout, and return
                await WebAuthenticator.Default.AuthenticateAsync(logoutRequest);

                System.Diagnostics.Debug.WriteLine("Mobile session cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Mobile logout error (expected): {ex.Message}");
                // Logout "errors" are normal as the callback doesn't return auth data
            }
        }

        // Keep all your existing helper methods (GenerateCodeVerifier, etc.)
        private async Task<string?> ExchangeCodeForTokenAsync(string authorizationCode, string codeVerifier, string? callbackUrl = null)
        {
            // ... your existing token exchange code ...
            try
            {
                var tokenUrl = $"https://{_domain}/oauth/token";
                var redirectUri = callbackUrl ?? _callbackUrl;

                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "authorization_code"),
                    new("client_id", _clientId),
                    new("code", authorizationCode),
                    new("redirect_uri", redirectUri),
                    new("code_verifier", codeVerifier)
                };

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var content = new FormUrlEncodedContent(formData);

                var response = await client.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var payload = JsonDocument.Parse(responseContent);
                if (payload.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    return tokenElement.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token exchange error: {ex.Message}");
                return null;
            }
        }

        private string GenerateState()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(challengeBytes);
        }

        private string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string AuthorizationCode { get; set; } = "";
        public string State { get; set; } = "";
    }
}