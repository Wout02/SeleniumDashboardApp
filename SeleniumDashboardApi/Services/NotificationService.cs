using Microsoft.AspNetCore.SignalR;
using SeleniumDashboardApi.Hubs;

namespace SeleniumDashboardApi.Services
{
    public interface INotificationService
    {
        Task NotifyNewTestRun(object testRun);
        Task NotifyTestRunUpdate(string testRunId, object update);
        Task NotifyTestRunCompleted(string testRunId, object result);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<TestRunHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<TestRunHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyNewTestRun(object testRun)
        {
            try
            {
                _logger.LogInformation("Sending new test run notification");

                await _hubContext.Clients.All.SendAsync("NewTestRun", testRun);

                _logger.LogInformation("New test run notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new test run notification");
            }
        }

        public async Task NotifyTestRunUpdate(string testRunId, object update)
        {
            try
            {
                _logger.LogInformation($"Sending test run update notification for {testRunId}");

                await _hubContext.Clients.All.SendAsync("TestRunUpdate", new { testRunId, update });

                _logger.LogInformation($"Test run update notification sent for {testRunId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send test run update notification for {testRunId}");
            }
        }

        public async Task NotifyTestRunCompleted(string testRunId, object result)
        {
            try
            {
                _logger.LogInformation($"Sending test run completion notification for {testRunId}");

                await _hubContext.Clients.All.SendAsync("TestRunCompleted", new { testRunId, result });

                _logger.LogInformation($"Test run completion notification sent for {testRunId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send test run completion notification for {testRunId}");
            }
        }
    }
}