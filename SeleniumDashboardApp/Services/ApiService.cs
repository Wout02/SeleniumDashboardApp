using Newtonsoft.Json;
using SeleniumDashboard.Shared;
using System.Net.Http.Headers;

namespace SeleniumDashboardApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;

            // Set authentication header on initialization
            SetAuthenticationHeader();
        }

        private void SetAuthenticationHeader()
        {
            try
            {
                var token = Preferences.Get("access_token", null);

                if (!string.IsNullOrEmpty(token))
                {
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    System.Diagnostics.Debug.WriteLine($"[API] Authentication header set with token: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                    System.Diagnostics.Debug.WriteLine("[API] No token found, authentication header cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error setting auth header: {ex.Message}");
            }
        }

        // Public method to refresh token after login
        public void RefreshAuthenticationToken()
        {
            System.Diagnostics.Debug.WriteLine("[API] Refreshing authentication token...");
            SetAuthenticationHeader();
        }

        // Alternative: Set token directly
        public void SetToken(string token)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    System.Diagnostics.Debug.WriteLine($"[API] Token set directly: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                    System.Diagnostics.Debug.WriteLine("[API] Token cleared");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Error setting token: {ex.Message}");
            }
        }

        public async Task<List<TestRun>> GetTestRunsAsync()
        {
            try
            {
                // Ensure we have the latest token before making the call
                SetAuthenticationHeader();

                System.Diagnostics.Debug.WriteLine("[API] Getting test runs...");
                var response = await _http.GetAsync("api/testrun");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        System.Diagnostics.Debug.WriteLine("[API] Unauthorized - token might be invalid");
                    }

                    return new List<TestRun>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TestRun>>(json) ?? new List<TestRun>();

                System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved {result.Count} test runs");
                return result;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] HTTP: {ex.Message}");
                return new List<TestRun>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API ERROR] General: {ex.Message}");
                return new List<TestRun>();
            }
        }

        public async Task<string> GetRawAsync(string endpoint)
        {
            SetAuthenticationHeader();
            var response = await _http.GetAsync(endpoint);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<TestRun?> GetTestRunAsync(int id)
        {
            try
            {
                SetAuthenticationHeader();

                System.Diagnostics.Debug.WriteLine($"[API] Getting test run {id}...");
                var response = await _http.GetAsync($"api/testrun/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Error getting test run {id}: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TestRun>(json);

                System.Diagnostics.Debug.WriteLine($"[API] Successfully retrieved test run {id}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GET SINGLE TESTRUN ERROR] {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTestRunAsync(int id)
        {
            try
            {
                SetAuthenticationHeader();

                System.Diagnostics.Debug.WriteLine($"[API] Deleting test run {id}...");
                var response = await _http.DeleteAsync($"api/testrun/{id}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Successfully deleted test run {id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[API] Failed to delete test run {id}: {response.StatusCode}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DELETE ERROR] {ex.Message}");
                return false;
            }
        }
    }
}