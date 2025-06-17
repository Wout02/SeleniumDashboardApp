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
        private readonly string _callbackUrl = "https://seleniumdashboardapp-production.up.railway.app/auth/callback"; // HTTPS callback naar API

        public async Task<string?> LoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== AUTH SERVICE: LoginAsync gestart ===");

#if WINDOWS
            System.Diagnostics.Debug.WriteLine("=== WINDOWS: Using system browser workaround ===");
            try
            {
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

                System.Diagnostics.Debug.WriteLine($"Opening system browser with Auth URL");
                
                // Open system browser instead of WebView (Windows WebAuthenticator workaround)
                var success = await Launcher.Default.OpenAsync(new Uri(authUrl));
                
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to launch system browser");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine("System browser opened successfully");
                System.Diagnostics.Debug.WriteLine("=== WINDOWS AUTH FLOW ===");
                System.Diagnostics.Debug.WriteLine("1. User logs in via browser");
                System.Diagnostics.Debug.WriteLine("2. Browser redirects to API callback");
                System.Diagnostics.Debug.WriteLine("3. For development: using placeholder token");
                System.Diagnostics.Debug.WriteLine("4. TODO: Implement callback server for production");
                
                // In development: placeholder token that works with your API
                return "dev-windows-auth-token-" + DateTime.Now.Ticks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows browser auth error: {ex.Message}");
                return null;
            }
#else

            try
            {
                System.Diagnostics.Debug.WriteLine($"Platform: {DeviceInfo.Platform}");
                System.Diagnostics.Debug.WriteLine("Genereren van PKCE parameters...");
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(codeVerifier);

                System.Diagnostics.Debug.WriteLine($"Code verifier: {codeVerifier.Substring(0, 10)}...");
                System.Diagnostics.Debug.WriteLine($"Code challenge: {codeChallenge.Substring(0, 10)}...");

                var authUrl = $"https://{_domain}/authorize" +
                              $"?client_id={Uri.EscapeDataString(_clientId)}" +
                              $"&response_type=code" +
                              $"&scope={Uri.EscapeDataString("openid profile email")}" +
                              $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
                              $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                              $"&code_challenge_method=S256" +
                              $"&prompt=login"; // Force fresh login

                System.Diagnostics.Debug.WriteLine($"Auth URL: {authUrl}");
                System.Diagnostics.Debug.WriteLine("Starten van WebAuthenticator...");

                // Platform-specific callback URL setup
                var callbackScheme = "mauiapp://callback";

#if WINDOWS
                System.Diagnostics.Debug.WriteLine("=== WINDOWS AUTH0 SETUP ===");
                // Windows needs specific WebAuthenticator configuration
                var authRequest = new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(callbackScheme),
                    PrefersEphemeralWebBrowserSession = false // Windows works better with false
                };
#else
                System.Diagnostics.Debug.WriteLine("=== ANDROID AUTH0 SETUP ===");
                var authRequest = new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(callbackScheme),
                    PrefersEphemeralWebBrowserSession = true // Android works better with true
                };
#endif

                System.Diagnostics.Debug.WriteLine($"=== WEBAUTH REQUEST DETAILS ===");
                System.Diagnostics.Debug.WriteLine($"Auth URL: {authRequest.Url}");
                System.Diagnostics.Debug.WriteLine($"Callback URL: {authRequest.CallbackUrl}");
                System.Diagnostics.Debug.WriteLine($"Ephemeral Session: {authRequest.PrefersEphemeralWebBrowserSession}");
                System.Diagnostics.Debug.WriteLine($"Platform: {DeviceInfo.Platform}");

                var result = await WebAuthenticator.Default.AuthenticateAsync(authRequest);

                System.Diagnostics.Debug.WriteLine("WebAuthenticator voltooid!");

                if (result?.Properties == null)
                {
                    System.Diagnostics.Debug.WriteLine("FOUT: Geen result properties ontvangen");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Properties ontvangen: {result.Properties.Count}");
                foreach (var prop in result.Properties)
                {
                    System.Diagnostics.Debug.WriteLine($"Property: {prop.Key} = {prop.Value}");
                }

                // Check for error first
                if (result.Properties.TryGetValue("error", out var error))
                {
                    System.Diagnostics.Debug.WriteLine($"Auth error: {error}");
                    if (result.Properties.TryGetValue("error_description", out var errorDesc))
                    {
                        System.Diagnostics.Debug.WriteLine($"Error description: {errorDesc}");
                    }
                    return null;
                }

                if (!result.Properties.TryGetValue("code", out var authorizationCode) ||
                    string.IsNullOrEmpty(authorizationCode))
                {
                    System.Diagnostics.Debug.WriteLine("FOUT: Geen authorization code ontvangen");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Authorization code ontvangen: {authorizationCode.Substring(0, 10)}...");

                // Token ophalen
                System.Diagnostics.Debug.WriteLine("Starten van token exchange...");
                var token = await ExchangeCodeForTokenAsync(authorizationCode, codeVerifier);

                if (!string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("=== TOKEN SUCCESVOL ONTVANGEN ===");
                    return token;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("=== TOKEN EXCHANGE MISLUKT ===");
                    return null;
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("=== LOGIN GEANNULEERD DOOR GEBRUIKER ===");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== LOGIN FOUT: {ex.GetType().Name}: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
#endif
        }

        private async Task<string?> ExchangeCodeForTokenAsync(string authorizationCode, string codeVerifier)
        {
            try
            {
                var tokenUrl = $"https://{_domain}/oauth/token";
                System.Diagnostics.Debug.WriteLine($"=== TOKEN EXCHANGE STARTING ===");
                System.Diagnostics.Debug.WriteLine($"Token URL: {tokenUrl}");
                System.Diagnostics.Debug.WriteLine($"Authorization Code: {authorizationCode}");
                System.Diagnostics.Debug.WriteLine($"Code Verifier: {codeVerifier}");

                // Use form-encoded data instead of JSON (Auth0 prefers this)
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "authorization_code"),
                    new("client_id", _clientId),
                    new("code", authorizationCode),
                    new("redirect_uri", _callbackUrl),
                    new("code_verifier", codeVerifier)
                };

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var content = new FormUrlEncodedContent(formData);

                System.Diagnostics.Debug.WriteLine("=== SENDING TOKEN REQUEST ===");
                foreach (var item in formData)
                {
                    var value = item.Key == "code" || item.Key == "code_verifier"
                        ? $"{item.Value.Substring(0, Math.Min(10, item.Value.Length))}..."
                        : item.Value;
                    System.Diagnostics.Debug.WriteLine($"{item.Key}: {value}");
                }

                var response = await client.PostAsync(tokenUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"=== TOKEN RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode} ({(int)response.StatusCode})");
                System.Diagnostics.Debug.WriteLine($"Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
                System.Diagnostics.Debug.WriteLine($"Body: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"=== TOKEN REQUEST FAILED ===");
                    System.Diagnostics.Debug.WriteLine($"Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Reason: {response.ReasonPhrase}");

                    // Try to parse error response
                    try
                    {
                        var errorDoc = JsonDocument.Parse(responseContent);
                        if (errorDoc.RootElement.TryGetProperty("error", out var errorElement))
                        {
                            System.Diagnostics.Debug.WriteLine($"Auth0 Error: {errorElement.GetString()}");
                        }
                        if (errorDoc.RootElement.TryGetProperty("error_description", out var errorDescElement))
                        {
                            System.Diagnostics.Debug.WriteLine($"Error Description: {errorDescElement.GetString()}");
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Could not parse error response as JSON");
                    }

                    return null;
                }

                var payload = JsonDocument.Parse(responseContent);
                if (payload.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    var token = tokenElement.GetString();
                    System.Diagnostics.Debug.WriteLine($"=== TOKEN SUCCESS ===");
                    System.Diagnostics.Debug.WriteLine($"Access token length: {token?.Length}");
                    System.Diagnostics.Debug.WriteLine($"Access token preview: {token?.Substring(0, Math.Min(30, token.Length))}...");

                    // Log other token properties
                    if (payload.RootElement.TryGetProperty("token_type", out var tokenType))
                    {
                        System.Diagnostics.Debug.WriteLine($"Token type: {tokenType.GetString()}");
                    }
                    if (payload.RootElement.TryGetProperty("expires_in", out var expiresIn))
                    {
                        System.Diagnostics.Debug.WriteLine($"Expires in: {expiresIn.GetInt32()} seconds");
                    }

                    return token;
                }

                System.Diagnostics.Debug.WriteLine("=== NO ACCESS TOKEN IN RESPONSE ===");
                System.Diagnostics.Debug.WriteLine("Available properties:");
                foreach (var prop in payload.RootElement.EnumerateObject())
                {
                    System.Diagnostics.Debug.WriteLine($"  {prop.Name}: {prop.Value}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== TOKEN EXCHANGE EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
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
}