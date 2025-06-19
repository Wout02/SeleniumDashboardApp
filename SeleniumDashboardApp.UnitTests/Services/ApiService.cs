using Newtonsoft.Json;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.UnitTests.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public virtual async Task<List<TestRun>> GetTestRunsAsync()
        {
            try
            {
                var json = await _http.GetStringAsync("api/testrun");
                return JsonConvert.DeserializeObject<List<TestRun>>(json);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API ERROR] {ex.Message}");
                return new List<TestRun>(); // fallback
            }
        }

        public virtual async Task<string> GetRawAsync(string endpoint)
        {
            return await _http.GetStringAsync(endpoint);
        }

        public virtual async Task<TestRun?> GetTestRunAsync(int id)
        {
            try
            {
                var json = await _http.GetStringAsync($"api/testrun/{id}");
                return JsonConvert.DeserializeObject<TestRun>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET SINGLE TESTRUN ERROR] {ex.Message}");
                return null;
            }
        }

        public virtual async Task<bool> DeleteTestRunAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/testrun/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE ERROR] {ex.Message}");
                return false;
            }
        }
    }
}