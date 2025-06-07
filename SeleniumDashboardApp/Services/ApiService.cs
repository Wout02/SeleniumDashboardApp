using Newtonsoft.Json;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestRun>> GetTestRunsAsync()
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

        public async Task<string> GetRawAsync(string endpoint)
        {
            return await _http.GetStringAsync(endpoint);
        }

        public async Task<TestRun?> GetTestRunAsync(int id)
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
    }
}
