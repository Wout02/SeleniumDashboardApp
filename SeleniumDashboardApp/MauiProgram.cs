using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
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
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7079");
            });
            builder.Services.AddSingleton<DashboardViewModel>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}