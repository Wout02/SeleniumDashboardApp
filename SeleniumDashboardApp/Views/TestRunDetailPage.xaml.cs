using SeleniumDashboardApp.Helpers;
using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Views.Tabs;

namespace SeleniumDashboardApp.Views;

public partial class TestRunDetailPage : ContentPage
{
    private readonly TabTemplateSelector _templateSelector;
    private readonly TestRunDetailViewModel _viewModel;

    public TestRunDetailPage(TestRunDetailViewModel viewModel, int testRunId)
    {
        try
        {
            Console.WriteLine($"[DEBUG] TestRunDetailPage constructor START - ID: {testRunId}");

            InitializeComponent();

            Console.WriteLine($"[DEBUG] InitializeComponent completed");

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel), "ViewModel cannot be null");

            _viewModel = viewModel;
            BindingContext = _viewModel;

            Console.WriteLine($"[DEBUG] ViewModel bound");

            _ = Task.Run(async () =>
            {
                try
                {
                    await _viewModel.LoadTestRunById(testRunId);
                    Console.WriteLine($"[DEBUG] Test run data loaded for ID: {testRunId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to load test run data: {ex.Message}");
                }
            });

            _templateSelector = new TabTemplateSelector
            {
                DetailsTemplate = new DataTemplate(typeof(DetailsView)),
                LogsTemplate = new DataTemplate(typeof(LogsView)),
                ChartsTemplate = new DataTemplate(typeof(ChartsView))
            };

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.SelectedTab))
                {
                    UpdateTabContent();
                }
            };

            UpdateTabContent();

            Console.WriteLine($"[DEBUG] TestRunDetailPage constructor COMPLETED - ID: {testRunId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] TestRunDetailPage constructor failed: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void UpdateTabContent()
    {
        try
        {
            Console.WriteLine($"[TAB] Updating content for: {_viewModel.SelectedTab}");

            var template = _templateSelector.SelectTemplate(_viewModel.SelectedTab, this);
            if (template == null)
            {
                Console.WriteLine($"[TAB ERROR] No template found for: {_viewModel.SelectedTab}");
                return;
            }

            var content = template.CreateContent();

            if (content is View view)
            {
                Console.WriteLine($"[TAB] View loaded: {view.GetType().Name}");
                view.BindingContext = _viewModel;
                TabContentView.Content = view;
            }
            else if (content is ViewCell cell && cell.View is View innerView)
            {
                Console.WriteLine($"[TAB] ViewCell loaded: {innerView.GetType().Name}");
                innerView.BindingContext = _viewModel;
                TabContentView.Content = innerView;
            }
            else
            {
                Console.WriteLine($"[TAB ERROR] Invalid content type: {content?.GetType().Name ?? "null"}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TAB ERROR] UpdateTabContent failed: {ex.Message}");
        }
    }
}