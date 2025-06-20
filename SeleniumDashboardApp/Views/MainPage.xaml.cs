using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.Views;
using Microsoft.Maui.Controls.Shapes;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.Views;

public partial class MainPage : ContentPage
{
    private readonly LocalDatabaseService _database;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApiService _apiService;
    private readonly ISignalRService _signalRService;
    private readonly ILocalNotificationService _notificationService;
    private readonly DashboardViewModel _viewModel;

    public MainPage(DashboardViewModel viewModel, LocalDatabaseService database, ApiService apiService, IServiceProvider serviceProvider, ISignalRService signalRService, ILocalNotificationService notificationService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _database = database;
        _apiService = apiService;
        _serviceProvider = serviceProvider;
        _signalRService = signalRService;
        _notificationService = notificationService;

        BackgroundColor = Colors.White;
        Title = "Selenium Dashboard";

        SetupSignalRNotifications();
    }

    private void SetupSignalRNotifications()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== SETTING UP SIGNALR NOTIFICATIONS ===");

            _signalRService.NewTestRunReceived += OnNewTestRunReceived;
            _signalRService.TestRunUpdateReceived += OnTestRunUpdateReceived;
            _signalRService.TestRunCompletedReceived += OnTestRunCompletedReceived;

            System.Diagnostics.Debug.WriteLine("SignalR event handlers registered");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up SignalR notifications: {ex.Message}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            System.Diagnostics.Debug.WriteLine("=== MAINPAGE APPEARING ===");

            await _notificationService.RequestPermissionAsync();
            System.Diagnostics.Debug.WriteLine("Notification permission requested");

            if (!_signalRService.IsConnected)
            {
                await _signalRService.StartAsync();
                System.Diagnostics.Debug.WriteLine($"SignalR connection started. Connected: {_signalRService.IsConnected}");
            }

            if (_viewModel != null)
            {
                await _viewModel.RefreshTestRuns();
                System.Diagnostics.Debug.WriteLine("Test runs refreshed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            System.Diagnostics.Debug.WriteLine("MainPage disappearing - keeping SignalR connected for background notifications");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
        }
    }

    private async void OnNewTestRunReceived(object? sender, TestRunNotification notification)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== NEW TEST RUN NOTIFICATION: {notification.ProjectName} ===");

            await _notificationService.ShowNotificationAsync(
                "🚀 Nieuwe Test Run",
                $"{notification.ProjectName} is gestart",
                notification.TestRunId
            );

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.RefreshTestRuns();
                    System.Diagnostics.Debug.WriteLine("Test runs refreshed after new test run notification");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling new test run notification: {ex.Message}");
        }
    }

    private async void OnTestRunUpdateReceived(object? sender, TestRunUpdateNotification notification)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== TEST RUN UPDATE NOTIFICATION: {notification.TestRunId} - {notification.Status} ===");

            if (notification.Status == "Running" || notification.Status == "Completed" || notification.Status == "Failed")
            {
                string icon = notification.Status switch
                {
                    "Running" => "▶️",
                    "Completed" => "✅",
                    "Failed" => "❌",
                    _ => "🔄"
                };

                await _notificationService.ShowNotificationAsync(
                    $"{icon} Test Update",
                    $"Test run status: {notification.Status} ({notification.Progress}%)",
                    notification.TestRunId
                );
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.RefreshTestRuns();
                    System.Diagnostics.Debug.WriteLine($"Test runs refreshed after update notification for {notification.TestRunId}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling test run update notification: {ex.Message}");
        }
    }

    private async void OnTestRunCompletedReceived(object? sender, TestRunCompletedNotification notification)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== TEST RUN COMPLETED NOTIFICATION: {notification.TestRunId} - Success: {notification.Success} ===");

            string icon = notification.Success ? "🎉" : "💥";
            string title = notification.Success ? "Test Succesvol!" : "Test Gefaald!";

            await _notificationService.ShowNotificationAsync(
                $"{icon} {title}",
                $"Summary: {notification.Summary}",
                notification.TestRunId
            );

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.RefreshTestRuns();
                    System.Diagnostics.Debug.WriteLine($"Test runs refreshed after completion notification for {notification.TestRunId}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling test run completion notification: {ex.Message}");
        }
    }

    private void OnToggleSearchClicked(object sender, EventArgs e)
    {
        SearchEntryFrame.IsVisible = !SearchEntryFrame.IsVisible;
    }

    private void OnToggleFilterClicked(object sender, EventArgs e)
    {
        StatusPickerFrame.IsVisible = !StatusPickerFrame.IsVisible;
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.RefreshTestRuns();
        }
    }

    private async void OnTestRunTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is TestRun selectedRun)
        {
            var apiService = _serviceProvider?.GetService<ApiService>() ?? _apiService;
            var viewModel = new TestRunDetailViewModel(apiService);

            var page = new TestRunDetailPage(viewModel, selectedRun.Id);
            await Navigation.PushAsync(page, animated: true);
        }
    }

    private async void OnDeleteTestRun(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is TestRun testRun)
        {
            var id = testRun.Id;
            var confirm = await DisplayAlert("Verwijderen", "Weet je zeker dat je deze testrun wilt verwijderen?", "Ja", "Annuleren");
            if (!confirm) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DELETING TEST RUN {id} ===");

                await _database.DeleteTestRunByIdAsync(id);

                if (BindingContext is DashboardViewModel viewModel)
                {
                    var testRunToRemove = viewModel.TestRuns.FirstOrDefault(t => t.Id == id);
                    if (testRunToRemove != null)
                    {
                        viewModel.TestRuns.Remove(testRunToRemove);
                        System.Diagnostics.Debug.WriteLine($"Removed test run {id} from UI collection");
                    }
                }

                var backendSuccess = await _apiService.DeleteTestRunAsync(id);

                if (backendSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Test run {id} successfully deleted from backend");
                    await DisplayAlert("Verwijderd", "Testrun is verwijderd.", "OK");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete test run {id} from backend");

                    if (BindingContext is DashboardViewModel vm)
                    {
                        await vm.RefreshTestRuns();
                    }

                    await DisplayAlert("Fout", "Kan testrun niet verwijderen uit backend. UI is hersteld.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting test run {id}: {ex.Message}");

                if (BindingContext is DashboardViewModel vm)
                {
                    await vm.RefreshTestRuns();
                }

                await DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }
    }

    ~MainPage()
    {
        try
        {
            _signalRService.NewTestRunReceived -= OnNewTestRunReceived;
            _signalRService.TestRunUpdateReceived -= OnTestRunUpdateReceived;
            _signalRService.TestRunCompletedReceived -= OnTestRunCompletedReceived;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainPage destructor: {ex.Message}");
        }
    }
}