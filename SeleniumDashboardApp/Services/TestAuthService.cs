using Microsoft.Maui.Authentication;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeleniumDashboardApp.Services
{
    public class TestAuthService
    {
        private readonly string _domain = "dev-14kcc8n2c231d8b8.eu.auth0.com";
        private readonly string _clientId = "OG71NTgV1UMVeKjytuKivGyTmmkkOlOw";
        private readonly string _callbackUrl = "https://yourapp.com/callback"; // HTTP callback voor testing

        public async Task<string?> LoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== TEST AUTH SERVICE: LoginAsync gestart ===");

#if WINDOWS
            System.Diagnostics.Debug.WriteLine("Windows platform - fake token wordt gebruikt");
            return "fake-token";
#else
            try
            {
                System.Diagnostics.Debug.WriteLine("Genereren van PKCE parameters...");
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

                System.Diagnostics.Debug.WriteLine($"Test Auth URL: {authUrl}");
                System.Diagnostics.Debug.WriteLine("Starten van WebAuthenticator met HTTP callback...");

                var authRequest = new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(_callbackUrl),
                    PrefersEphemeralWebBrowserSession = true
                };

                var result = await WebAuthenticator.Default.AuthenticateAsync(authRequest);

                System.Diagnostics.Debug.WriteLine("Test WebAuthenticator voltooid!");

                if (result?.Properties == null)
                {
                    System.Diagnostics.Debug.WriteLine("FOUT: Geen result properties ontvangen");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Test Properties ontvangen: {result.Properties.Count}");
                foreach (var prop in result.Properties)
                {
                    System.Diagnostics.Debug.WriteLine($"Test Property: {prop.Key} = {prop.Value}");
                }

                // Voor HTTP callback, zoek naar 'code' parameter
                if (!result.Properties.TryGetValue("code", out var authorizationCode) || 
                    string.IsNullOrEmpty(authorizationCode))
                {
                    System.Diagnostics.Debug.WriteLine("FOUT: Geen authorization code ontvangen");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Test Authorization code ontvangen: {authorizationCode.Substring(0, 10)}...");

                // Return een test token
                return $"test_token_{DateTime.Now.Ticks}";
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("=== TEST LOGIN GEANNULEERD ===");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== TEST LOGIN FOUT: {ex.GetType().Name}: {ex.Message} ===");
                return null;
            }
#endif
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