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
            var json = await _http.GetStringAsync("api/testrun");
            return JsonConvert.DeserializeObject<List<TestRun>>(json);
        }
    }
}