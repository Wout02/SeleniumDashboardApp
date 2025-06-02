using Microsoft.Extensions.Logging;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Views;
using CommunityToolkit.Maui;
using SeleniumDashboardApp.ViewModels;

namespace SeleniumDashboardApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "testruns.db3");

        // Services
        builder.Services.AddSingleton(new LocalDatabaseService(dbPath));
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://cf55-84-83-178-6.ngrok-free.app/");
        });

        // ViewModels
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddTransient<TestRunDetailViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<TestRunDetailPage>();

#if DEBUG
        builder.Configuration["DisableDebugToolbar"] = "true";
#endif

        return builder.Build();
    }
}