﻿using Microsoft.Maui.Storage;
using Microsoft.Maui.Dispatching;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    private async Task RefreshApiServiceAfterLogin(string newToken)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== REFRESHING API SERVICE WITH NEW TOKEN ===");

            var serviceProvider = IPlatformApplication.Current?.Services;
            if (serviceProvider != null)
            {
                var apiService = serviceProvider.GetService<ApiService>();
                if (apiService != null)
                {
                    apiService.SetToken(newToken);

                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Testing API service with new token...");
                        var testResult = await apiService.GetTestRunsAsync();
                        System.Diagnostics.Debug.WriteLine($"API test successful: {testResult?.Count ?? 0} runs");
                    }
                    catch (Exception apiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"API test failed: {apiEx.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Could not get ApiService from service provider");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Service provider is null");
            }

            System.Diagnostics.Debug.WriteLine("=== API SERVICE REFRESH COMPLETED ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing API service: {ex.Message}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();

        // Create a temporary loading page
        var loadingPage = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Laden...",
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    }
                }
            }
        };

        window.Page = loadingPage;

        // Execute startup on main thread with delay
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Small delay to ensure window is fully initialized
            await Task.Delay(100);
            await StartupAsync(window);
        });

        return window;
    }

    private async Task StartupAsync(Window window)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== APP STARTUP: Begin (Main Thread) ===");

            var authService = new AuthService();
            string? token = Preferences.Get("access_token", null);

            var wasLoggedOut = Preferences.Get("user_logged_out", "false");

            System.Diagnostics.Debug.WriteLine($"Bestaande token gevonden: {!string.IsNullOrEmpty(token)}");
            System.Diagnostics.Debug.WriteLine($"Was logged out: {wasLoggedOut}");

            if (wasLoggedOut == "true")
            {
                System.Diagnostics.Debug.WriteLine("User was logged out - starting direct login");

                Preferences.Remove("user_logged_out");

                await StartDirectLogin(window, authService);
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("Geen token gevonden, starten direct login flow...");

                await StartDirectLogin(window, authService);
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Token beschikbaar, refreshing API service...");
                await RefreshApiServiceAfterLogin(token);

                window.Page = new AppShell();
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== APP STARTUP FOUT: {ex} ===");
            window.Page = new ContentPage
            {
                Content = new Label
                {
                    Text = $"App startup fout: {ex.Message}",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };
        }
    }

    private async Task StartDirectLogin(Window window, AuthService authService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== STARTING DIRECT LOGIN (Main Thread) ===");

            window.Page = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 20,
                    Children =
                    {
                        new ActivityIndicator
                        {
                            IsRunning = true,
                            Color = Colors.Blue
                        },
                        new Label
                        {
                            Text = "Inloggen...",
                            HorizontalOptions = LayoutOptions.Center,
                            FontSize = 18
                        },
#if WINDOWS
                        new Label
                        {
                            Text = "Windows: WebView Auth0 login",
                            FontSize = 12,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        }
#else
                        new Label
                        {
                            Text = "",
                            FontSize = 12,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        }
#endif
                    }
                }
            };

            var token = await authService.LoginAsync();

            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set("access_token", token);
                System.Diagnostics.Debug.WriteLine("Direct login successful, refreshing API service...");

                await RefreshApiServiceAfterLogin(token);

                window.Page = new AppShell();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Direct login failed");

                window.Page = new ContentPage
                {
                    Content = new VerticalStackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 20,
                        Children =
                        {
                            new Label
                            {
                                Text = "❌ Login mislukt",
                                FontSize = 18,
                                HorizontalOptions = LayoutOptions.Center,
                                TextColor = Colors.Red
                            },
                            new Label
                            {
                                Text = "Probeer het opnieuw door de app te herstarten",
                                HorizontalOptions = LayoutOptions.Center,
                                HorizontalTextAlignment = TextAlignment.Center
                            }
                        }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Direct login error: {ex.Message}");

            window.Page = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 20,
                    Children =
                    {
                        new Label
                        {
                            Text = "❌ Login fout",
                            FontSize = 18,
                            HorizontalOptions = LayoutOptions.Center,
                            TextColor = Colors.Red
                        },
                        new Label
                        {
                            Text = ex.Message,
                            HorizontalOptions = LayoutOptions.Center,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    }
                }
            };
        }
    }
}