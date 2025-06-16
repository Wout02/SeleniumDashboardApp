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

                System.Diagnostics.Debug.WriteLine($"Bestaande token gevonden: {!string.IsNullOrEmpty(token)}");

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
                System.Diagnostics.Debug.WriteLine("Tonen van error pagina");
                MainThread.BeginInvokeOnMainThread(() =>
                {
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
                                    Text = "Login mislukt of geannuleerd.",
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    FontSize = 18
                                },
                                new Label
                                {
                                    Text = "Controleer je internetverbinding en probeer opnieuw.",
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    FontSize = 14,
                                    TextColor = Colors.Gray
                                },
                                new Button
                                {
                                    Text = "Opnieuw proberen",
                                    Command = new Command(async () =>
                                    {
                                        System.Diagnostics.Debug.WriteLine("Retry button clicked");
                                        Preferences.Remove("access_token");

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
                                                        Text = "Opnieuw proberen...",
                                                        HorizontalOptions = LayoutOptions.Center,
                                                        Margin = new Thickness(0, 10, 0, 0)
                                                    }
                                                }
                                            }
                                        };

                                        try
                                        {
                                            System.Diagnostics.Debug.WriteLine("Starting retry login...");
                                            var retryToken = await authService.LoginAsync();
                                            if (!string.IsNullOrEmpty(retryToken))
                                            {
                                                Preferences.Set("access_token", retryToken);
                                                MainThread.BeginInvokeOnMainThread(() =>
                                                {
                                                    window.Page = new AppShell();
                                                });
                                            }
                                            else
                                            {
                                                System.Diagnostics.Debug.WriteLine("Retry login failed - no token received");
                                                MainThread.BeginInvokeOnMainThread(async () =>
                                                {
                                                    await Application.Current.MainPage.DisplayAlert("Mislukt", "Login is nog steeds niet gelukt. Controleer je internetverbinding en Auth0 configuratie.", "OK");
                                                });
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Retry failed: {ex.Message}");
                                            MainThread.BeginInvokeOnMainThread(async () =>
                                            {
                                                await Application.Current.MainPage.DisplayAlert("Fout", $"Login mislukt: {ex.Message}", "OK");
                                            });
                                        }
                                    })
                                }
                            }
                        }
                    };
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
}