﻿using Microsoft.Extensions.Logging;
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
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "testruns.db3");

        builder.Services.AddSingleton(new LocalDatabaseService(dbPath));
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://seleniumdashboardapp-production.up.railway.app");
        });
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ISignalRService, SignalRService>();
        builder.Services.AddSingleton<ILocalNotificationService, SimpleRealNotificationService>();

        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddTransient<TestRunDetailViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<TestRunDetailPage>();


#if DEBUG
        builder.Configuration["DisableDebugToolbar"] = "true";
#endif

        return builder.Build();
    }
}