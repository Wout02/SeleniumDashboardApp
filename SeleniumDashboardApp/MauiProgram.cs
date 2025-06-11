// MauiProgram.cs - Fix for Microcharts.Maui 1.0.0
using Microsoft.Extensions.Logging;
using SeleniumDashboardApp.Services;
using SeleniumDashboardApp.ViewModels;
using SeleniumDashboardApp.Views;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microcharts.Maui;
using SeleniumDashboardApp.Views.Tabs;

namespace SeleniumDashboardApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .UseMicrocharts() // This should work with 1.0.0
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "testruns.db3");

        // Services
        builder.Services.AddSingleton(new LocalDatabaseService(dbPath));
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://dc49-84-83-178-6.ngrok-free.app/");
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