using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SeleniumDashboardApp.Views.Tabs;

public partial class LogsView : ContentView
{
    public LogsView()
    {
        InitializeComponent();

        this.BindingContextChanged += (_, __) =>
        {
            System.Diagnostics.Debug.WriteLine($"[LOGSVIEW] BindingContext updated to: {BindingContext?.GetType().Name ?? "null"}");
        };
    }

    private async void OnCopyClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(LogEditor.Text))
        {
            await Clipboard.SetTextAsync(LogEditor.Text);
            await Application.Current.MainPage.DisplayAlert("Gekopieerd", "Log is naar het klembord gekopieerd.", "OK");
        }
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        try
        {
            var filename = $"testrun-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            var path = Path.Combine(FileSystem.CacheDirectory, filename);

            File.WriteAllText(path, LogEditor.Text ?? "");

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Logbestand delen",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Fout", $"Kon log niet downloaden: {ex.Message}", "OK");
        }
    }
}