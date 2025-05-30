using Microsoft.Extensions.Logging;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Views;
using SeleniumDashboardApp.Models;

namespace SeleniumDashboardApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "testruns.db3");

            builder.Services.AddSingleton(new LocalDatabaseService(dbPath));

            // ✅ Correcte registratie van ApiService met BaseAddress
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("http://5af2-84-83-178-6.ngrok-free.app/");
            });

            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Configuration["DisableDebugToolbar"] = "true";
#endif

            return builder.Build();
        }
    }
}