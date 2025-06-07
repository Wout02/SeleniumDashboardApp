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
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Testrun + logs laden via API
        Console.WriteLine($"[DEBUG] TestRunDetailPage constructor - ID: {testRunId}");
        _ = _viewModel.LoadTestRunById(testRunId);

        // Tabs instellen
        _templateSelector = new TabTemplateSelector
        {
            DetailsTemplate = new DataTemplate(typeof(DetailsView)),
            LogsTemplate = new DataTemplate(typeof(LogsView)),
            ChartsTemplate = new DataTemplate(typeof(ChartsView))
        };

        // Reageer op tabwijziging
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.SelectedTab))
            {
                UpdateTabContent();
            }
        };

        // Initieel content instellen
        UpdateTabContent();
    }

    private void UpdateTabContent()
    {
        var template = _templateSelector.SelectTemplate(_viewModel.SelectedTab, this);
        var content = template.CreateContent();

        if (content is View view)
        {
            view.BindingContext = _viewModel;
            TabContentView.Content = view;
        }
        else if (content is ViewCell cell && cell.View is View innerView)
        {
            innerView.BindingContext = _viewModel;
            TabContentView.Content = innerView;
        }
    }
}