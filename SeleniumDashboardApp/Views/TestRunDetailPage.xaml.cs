using SeleniumDashboardApp.Helpers;
using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Views.Tabs;

namespace SeleniumDashboardApp.Views;

public partial class TestRunDetailPage : ContentPage
{
    private readonly TabTemplateSelector _templateSelector;
    private readonly TestRunDetailViewModel _viewModel;

    public TestRunDetailPage(TestRunDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Init TabTemplateSelector
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
        var content = _templateSelector
            .SelectTemplate(_viewModel.SelectedTab, this)
            .CreateContent() as View;

        TabContentView.Content = content;
    }
}