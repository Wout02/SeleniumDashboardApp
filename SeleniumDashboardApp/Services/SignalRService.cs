using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace SeleniumDashboardApp.Services
{
    public interface ISignalRService
    {
        Task StartAsync();
        Task StopAsync();
        bool IsConnected { get; }
        event EventHandler<TestRunNotification>? NewTestRunReceived;
        event EventHandler<TestRunUpdateNotification>? TestRunUpdateReceived;
        event EventHandler<TestRunCompletedNotification>? TestRunCompletedReceived;
    }

    public class SignalRService : ISignalRService, IDisposable
    {
        private HubConnection? _connection;
        private readonly string _hubUrl = "https://seleniumdashboardapp-production.up.railway.app/testrunhub";

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public event EventHandler<TestRunNotification>? NewTestRunReceived;
        public event EventHandler<TestRunUpdateNotification>? TestRunUpdateReceived;
        public event EventHandler<TestRunCompletedNotification>? TestRunCompletedReceived;

        public async Task StartAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SIGNALR: Starting connection ===");

                _connection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                    .Build();

                // Event handlers voor notificaties
                _connection.On<JsonElement>("NewTestRun", (testRunData) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"=== NEW TEST RUN RECEIVED: {testRunData} ===");

                        var notification = new TestRunNotification
                        {
                            TestRunId = testRunData.GetProperty("id").GetString() ?? "",
                            ProjectName = testRunData.GetProperty("projectName").GetString() ?? "",
                            Status = testRunData.GetProperty("status").GetString() ?? "",
                            Date = testRunData.GetProperty("date").GetDateTime(),
                            Summary = testRunData.GetProperty("summary").GetString() ?? "",
                            Message = "Nieuwe test run gestart!"
                        };

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            NewTestRunReceived?.Invoke(this, notification);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing new test run: {ex.Message}");
                    }
                });

                _connection.On<JsonElement>("TestRunUpdate", (updateData) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"=== TEST RUN UPDATE RECEIVED ===");

                        var notification = new TestRunUpdateNotification
                        {
                            TestRunId = updateData.GetProperty("testRunId").GetString() ?? "",
                            Status = updateData.GetProperty("update").GetProperty("status").GetString() ?? "",
                            Progress = updateData.GetProperty("update").GetProperty("progress").GetInt32(),
                            Message = "Test run bijgewerkt"
                        };

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            TestRunUpdateReceived?.Invoke(this, notification);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing test run update: {ex.Message}");
                    }
                });

                _connection.On<JsonElement>("TestRunCompleted", (completionData) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"=== TEST RUN COMPLETED RECEIVED ===");

                        var notification = new TestRunCompletedNotification
                        {
                            TestRunId = completionData.GetProperty("testRunId").GetString() ?? "",
                            Success = completionData.GetProperty("result").GetProperty("success").GetBoolean(),
                            Summary = completionData.GetProperty("result").GetProperty("summary").GetString() ?? "",
                            Message = completionData.GetProperty("result").GetProperty("success").GetBoolean()
                                ? "Test run succesvol voltooid!"
                                : "Test run gefaald!"
                        };

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            TestRunCompletedReceived?.Invoke(this, notification);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing test run completion: {ex.Message}");
                    }
                });

                // Connection event handlers
                _connection.Reconnecting += (error) =>
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR reconnecting: {error?.Message}");
                    return Task.CompletedTask;
                };

                _connection.Reconnected += (connectionId) =>
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR reconnected: {connectionId}");
                    return Task.CompletedTask;
                };

                _connection.Closed += (error) =>
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR connection closed: {error?.Message}");
                    return Task.CompletedTask;
                };

                await _connection.StartAsync();

                // Join all-users group
                await _connection.InvokeAsync("JoinGroup", "all-users");

                System.Diagnostics.Debug.WriteLine("=== SIGNALR: Connected successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== SIGNALR CONNECTION ERROR: {ex.Message} ===");
            }
        }

        public async Task StopAsync()
        {
            if (_connection is not null)
            {
                try
                {
                    await _connection.StopAsync();
                    System.Diagnostics.Debug.WriteLine("=== SIGNALR: Disconnected ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== SIGNALR DISCONNECT ERROR: {ex.Message} ===");
                }
            }
        }

        public void Dispose()
        {
            _connection?.DisposeAsync();
        }
    }

    // Notification models
    public class TestRunNotification
    {
        public string TestRunId { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime Date { get; set; }
        public string Summary { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class TestRunUpdateNotification
    {
        public string TestRunId { get; set; } = "";
        public string Status { get; set; } = "";
        public int Progress { get; set; }
        public string Message { get; set; } = "";
    }

    public class TestRunCompletedNotification
    {
        public string TestRunId { get; set; } = "";
        public bool Success { get; set; }
        public string Summary { get; set; } = "";
        public string Message { get; set; } = "";
    }
}