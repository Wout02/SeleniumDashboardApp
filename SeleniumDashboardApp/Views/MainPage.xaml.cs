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

        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.SelectedStatus))
                UpdateStatusDisplay(viewModel.SelectedStatus);
        };

        // Subscribe to SignalR notifications
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

            // Request notification permissions
            await _notificationService.RequestPermissionAsync();
            System.Diagnostics.Debug.WriteLine("Notification permission requested");

            // Start SignalR connection
            if (!_signalRService.IsConnected)
            {
                await _signalRService.StartAsync();
                System.Diagnostics.Debug.WriteLine($"SignalR connection started. Connected: {_signalRService.IsConnected}");
            }

            // Refresh data
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
            // Don't disconnect SignalR when page disappears - keep it running for background notifications
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

            // Show local notification
            await _notificationService.ShowNotificationAsync(
                "🚀 Nieuwe Test Run",
                $"{notification.ProjectName} is gestart",
                notification.TestRunId
            );

            // Refresh the UI data
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

            // Only show notification for significant status changes
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

            // Refresh the UI data
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

            // Refresh the UI data
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

    private void UpdateStatusDisplay(string status)
    {
        StatusDisplay.Text = string.IsNullOrWhiteSpace(status) ? string.Empty : $"Status: {status}";
        StatusDisplay.IsVisible = !string.IsNullOrWhiteSpace(status);
    }

    private void OnToggleSearchClicked(object sender, EventArgs e)
    {
        SearchEntryFrame.IsVisible = !SearchEntryFrame.IsVisible;
    }

    private void OnToggleFilterClicked(object sender, EventArgs e)
    {
        StatusPickerFrame.IsVisible = !StatusPickerFrame.IsVisible;
    }

    private async void OnTestRunTapped(object sender, SelectionChangedEventArgs e)
    {
        Console.WriteLine("TestRun geklikt");

        if (e.CurrentSelection.FirstOrDefault() is TestRun selectedRun)
        {
            Console.WriteLine($"Geselecteerde run: {selectedRun.Id}");

            var viewModel = _serviceProvider.GetService<TestRunDetailViewModel>();

            var page = new TestRunDetailPage(viewModel, selectedRun.Id);
            await Navigation.PushAsync(page, animated: true);
        }

        TestRunsCollectionView.SelectedItem = null;
    }

    private async void OnDeleteTestRun(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int id)
        {
            var confirm = await DisplayAlert("Verwijderen", "Weet je zeker dat je deze testrun wilt verwijderen?", "Ja", "Annuleren");
            if (!confirm) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DELETING TEST RUN {id} ===");

                // First, remove from local database to give immediate UI feedback
                await _database.DeleteTestRunByIdAsync(id);

                // Update UI immediately
                if (BindingContext is DashboardViewModel viewModel)
                {
                    // Remove from local collection first for immediate feedback
                    var testRunToRemove = viewModel.TestRuns.FirstOrDefault(t => t.Id == id);
                    if (testRunToRemove != null)
                    {
                        viewModel.TestRuns.Remove(testRunToRemove);
                        System.Diagnostics.Debug.WriteLine($"Removed test run {id} from UI collection");
                    }
                }

                // Then call backend (SignalR will handle notifying other clients)
                var backendSuccess = await _apiService.DeleteTestRunAsync(id);

                if (backendSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"Test run {id} successfully deleted from backend");
                    await DisplayAlert("Verwijderd", "Testrun is verwijderd.", "OK");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete test run {id} from backend");

                    // If backend delete failed, refresh to restore correct state
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

                // On error, refresh to restore correct state
                if (BindingContext is DashboardViewModel vm)
                {
                    await vm.RefreshTestRuns();
                }

                await DisplayAlert("Fout", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }
    }

    // Cleanup when page is destroyed
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