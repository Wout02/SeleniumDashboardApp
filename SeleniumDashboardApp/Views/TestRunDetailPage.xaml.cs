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
        Console.WriteLine($"[TAB] Gewijzigd naar: {_viewModel.SelectedTab}");

        var template = _templateSelector.SelectTemplate(_viewModel.SelectedTab, this);
        var content = template.CreateContent();

        if (content is View view)
        {
            Console.WriteLine($"[TAB] View geladen: {view.GetType().Name}");
            view.BindingContext = _viewModel;
            TabContentView.Content = view;
        }
        else if (content is ViewCell cell && cell.View is View innerView)
        {
            Console.WriteLine($"[TAB] ViewCell geladen: {innerView.GetType().Name}");
            innerView.BindingContext = _viewModel;
            TabContentView.Content = innerView;
        }
        else
        {
            Console.WriteLine($"[TAB] Geen geldige content geladen.");
        }
    }
}