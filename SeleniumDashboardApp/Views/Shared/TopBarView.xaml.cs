using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp.Views.Shared
{
    public partial class TopBarView : ContentView
    {
        public TopBarView()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Preferences.Remove("access_token");

            try
            {
                var authService = new AuthService();
                var token = await authService.LoginAsync();

                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("access_token", token);
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Login geannuleerd", "Je bent niet ingelogd", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Fout", $"Login mislukt: {ex.Message}", "OK");
            }
        }
    }
}