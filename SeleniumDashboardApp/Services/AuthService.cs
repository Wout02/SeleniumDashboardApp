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
                System.Diagnostics.Debug.WriteLine("=== STARTING WINDOWS WEBVIEW LOGIN ===");
                
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);
                var state = GenerateState();

                System.Diagnostics.Debug.WriteLine($"Code verifier: {codeVerifier.Substring(0, 10)}...");
                System.Diagnostics.Debug.WriteLine($"Code challenge: {codeChallenge.Substring(0, 10)}...");

                var windowsCallbackUrl = "mauiapp://callback";

                var authUrl = $"https://{_domain}/authorize" +
                              $"?client_id={Uri.EscapeDataString(_clientId)}" +
                              $"&response_type=code" +
                              $"&scope={Uri.EscapeDataString("openid profile email")}" +
                              $"&redirect_uri={Uri.EscapeDataString(windowsCallbackUrl)}" +
                              $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                              $"&code_challenge_method=S256" +
                              $"&state={Uri.EscapeDataString(state)}" +
                              $"&prompt=login";

                System.Diagnostics.Debug.WriteLine($"=== AUTH URL CREATED ===");
                System.Diagnostics.Debug.WriteLine($"URL: {authUrl}");

                System.Diagnostics.Debug.WriteLine("=== CREATING WEBVIEW PAGE ===");
                var tcs = new TaskCompletionSource<AuthResult>();
                var authPage = new Views.AuthWebViewPage(authUrl, windowsCallbackUrl, tcs);
                
                System.Diagnostics.Debug.WriteLine("=== PUSHING MODAL ON MAIN THREAD ===");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    // Wait for MainPage to be available
                    var retryCount = 0;
                    while (Application.Current?.MainPage == null && retryCount < 50)
                    {
                        System.Diagnostics.Debug.WriteLine($"Waiting for MainPage... attempt {retryCount + 1}");
                        await Task.Delay(100);
                        retryCount++;
                    }
                    
                    if (Application.Current?.MainPage == null)
                    {
                        throw new InvalidOperationException("MainPage is still null after waiting");
                    }
                    
                    System.Diagnostics.Debug.WriteLine("MainPage is available, pushing modal...");
                    await Application.Current.MainPage.Navigation.PushModalAsync(authPage);
                });
                
                System.Diagnostics.Debug.WriteLine("=== WAITING FOR AUTH RESULT ===");
                var authResult = await tcs.Task;
                
                System.Diagnostics.Debug.WriteLine($"=== AUTH RESULT RECEIVED ===");
                System.Diagnostics.Debug.WriteLine($"Success: {authResult.IsSuccess}");
                System.Diagnostics.Debug.WriteLine($"Error: {authResult.ErrorMessage}");
                System.Diagnostics.Debug.WriteLine($"Code present: {!string.IsNullOrEmpty(authResult.AuthorizationCode)}");
                
                if (!authResult.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"=== AUTH FAILED: {authResult.ErrorMessage} ===");
                    return null;
                }
                
                if (string.IsNullOrEmpty(authResult.AuthorizationCode))
                {
                    System.Diagnostics.Debug.WriteLine("=== NO AUTH CODE RECEIVED ===");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"=== STARTING TOKEN EXCHANGE ===");
                var token = await ExchangeCodeForTokenAsync(authResult.AuthorizationCode, codeVerifier, windowsCallbackUrl);
                
                if (!string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("=== TOKEN EXCHANGE SUCCESS ===");
                    Preferences.Set("user_name", "Auth0 User (Windows)");
                    Preferences.Set("user_email", "user@auth0.com");
                    return token;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("=== TOKEN EXCHANGE FAILED ===");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== EXCEPTION IN WINDOWS LOGIN ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
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
                    PrefersEphemeralWebBrowserSession = true
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
                try
                {
                    var serviceProvider = IPlatformApplication.Current?.Services;
                    var apiService = serviceProvider?.GetService<ApiService>();
                    if (apiService != null)
                    {
                        apiService.SetToken("");
                        System.Diagnostics.Debug.WriteLine("API Service token cleared");
                    }
                }
                catch (Exception apiEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing API service: {apiEx.Message}");
                }

                Preferences.Remove("access_token");
                Preferences.Remove("user_name");
                Preferences.Remove("user_email");

                Preferences.Set("user_logged_out", "true");

                System.Diagnostics.Debug.WriteLine("Local storage cleared and logout flag set");

                await Task.Delay(100);

                System.Diagnostics.Debug.WriteLine("Logout completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

        private async Task<string?> ExchangeCodeForTokenAsync(string authorizationCode, string codeVerifier, string? callbackUrl = null)
        {
            try
            {
                var tokenUrl = $"https://{_domain}/oauth/token";
                var redirectUri = callbackUrl ?? _callbackUrl;

                System.Diagnostics.Debug.WriteLine($"=== TOKEN EXCHANGE ===");
                System.Diagnostics.Debug.WriteLine($"Token URL: {tokenUrl}");
                System.Diagnostics.Debug.WriteLine($"Redirect URI: {redirectUri}");
                System.Diagnostics.Debug.WriteLine($"Client ID: {_clientId}");
                System.Diagnostics.Debug.WriteLine($"Auth Code: {authorizationCode.Substring(0, Math.Min(10, authorizationCode.Length))}...");

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

                System.Diagnostics.Debug.WriteLine($"Token response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Token response content: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Token exchange failed with status: {response.StatusCode}");
                    return null;
                }

                var payload = JsonDocument.Parse(responseContent);
                if (payload.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    var token = tokenElement.GetString();
                    System.Diagnostics.Debug.WriteLine($"Access token received: {(string.IsNullOrEmpty(token) ? "null" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
                    return token;
                }

                System.Diagnostics.Debug.WriteLine("No access_token found in response");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Token exchange error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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