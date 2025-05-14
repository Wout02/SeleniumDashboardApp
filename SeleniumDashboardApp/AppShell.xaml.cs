using SeleniumDashboardApp.Views;

namespace SeleniumDashboardApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
