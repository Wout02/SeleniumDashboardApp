namespace SeleniumDashboardApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Zorgt voor navigatie-ondersteuning
            return new Window(new NavigationPage(new AppShell()));
        }
    }
}