using Microsoft.Maui.Storage;
using Microsoft.Maui.Dispatching;
using SeleniumDashboardApp.Services;

namespace SeleniumDashboardApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window();

        // Zet tijdelijke laadscherm
        window.Page = new ContentPage
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

        _ = Task.Run(async () =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== APP STARTUP: Begin ===");

                var authService = new AuthService();
                string? token = Preferences.Get("access_token", null);

                // CHECK: Is de gebruiker recent uitgelogd?
                var wasLoggedOut = Preferences.Get("user_logged_out", "false");

                System.Diagnostics.Debug.WriteLine($"Bestaande token gevonden: {!string.IsNullOrEmpty(token)}");
                System.Diagnostics.Debug.WriteLine($"Was logged out: {wasLoggedOut}");

                // Als gebruiker recent uitgelogd is, toon login prompt in plaats van auto-login
                if (wasLoggedOut == "true")
                {
                    System.Diagnostics.Debug.WriteLine("User was logged out - showing login prompt");

                    // Clear the logout flag
                    Preferences.Remove("user_logged_out");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ShowLoginPrompt(window);
                    });
                    return;
                }

                // Login starten als er geen token is
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("Geen token gevonden, starten login flow...");

                    try
                    {
                        // Increased timeout to 5 minutes for login
                        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

                        System.Diagnostics.Debug.WriteLine("Starting login with 5 minute timeout...");
                        token = await authService.LoginAsync();

                        System.Diagnostics.Debug.WriteLine($"Login completed, token received: {!string.IsNullOrEmpty(token)}");

                        if (!string.IsNullOrEmpty(token))
                        {
                            Preferences.Set("access_token", token);
                            System.Diagnostics.Debug.WriteLine("Token opgeslagen, navigeren naar AppShell");

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                window.Page = new AppShell();
                            });
                            return;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Login geannuleerd of mislukt");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Login exception: {ex.GetType().Name}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    // Token is al beschikbaar
                    System.Diagnostics.Debug.WriteLine("Token beschikbaar, navigeren naar AppShell");
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        window.Page = new AppShell();
                    });
                    return;
                }

                // Als we hier komen: login faalde of werd geannuleerd
                System.Diagnostics.Debug.WriteLine("Tonen van login prompt");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShowLoginPrompt(window);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== APP STARTUP FOUT: {ex} ===");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    window.Page = new ContentPage
                    {
                        Content = new Label
                        {
                            Text = $"App startup fout: {ex.Message}",
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        }
                    };
                });
            }
        });

        return window;
    }

    private void ShowLoginPrompt(Window window)
    {
        window.Page = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 30,
                Padding = 40,
                Children =
                {
                    new Label
                    {
                        Text = "🔐 Selenium Dashboard",
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Colors.Blue
                    },
                    new Label
                    {
                        Text = "Welkom! Log in om je test resultaten te bekijken.",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Colors.Gray
                    },
#if WINDOWS
                    new Label
                    {
                        Text = "Windows Demo Mode: Automatische toegang",
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Colors.Orange,
                        FontAttributes = FontAttributes.Italic
                    },
#endif
                    new Button
                    {
                        Text = "🚀 Inloggen",
                        FontSize = 18,
                        BackgroundColor = Colors.Blue,
                        TextColor = Colors.White,
                        CornerRadius = 10,
                        Padding = new Thickness(30, 15),
                        Command = new Command(async () => await StartLoginFromPrompt(window))
                    }
                }
            }
        };
    }

    private async Task StartLoginFromPrompt(Window window)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== STARTING LOGIN FROM PROMPT ===");

            // Show loading
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
                            HorizontalOptions = LayoutOptions.Center
                        },
#if WINDOWS
                        new Label
                        {
                            Text = "Windows: Automatische demo login",
                            FontSize = 12,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        }
#else
                        new Label
                        {
                            Text = "Volg de instructies in je browser",
                            FontSize = 12,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        }
#endif
                    }
                }
            };

            var authService = new AuthService();
            var token = await authService.LoginAsync();

            if (!string.IsNullOrEmpty(token))
            {
                Preferences.Set("access_token", token);
                System.Diagnostics.Debug.WriteLine("Login from prompt successful");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    window.Page = new AppShell();
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Login from prompt failed");

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Fout", "Login mislukt", "OK");
                    ShowLoginPrompt(window);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login from prompt error: {ex.Message}");

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Fout", $"Login error: {ex.Message}", "OK");
                ShowLoginPrompt(window);
            });
        }
    }
}