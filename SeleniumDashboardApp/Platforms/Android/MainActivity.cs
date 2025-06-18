using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SeleniumDashboardApp.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        System.Diagnostics.Debug.WriteLine("=== MAINACTIVITY ONCREATE ===");
        System.Diagnostics.Debug.WriteLine($"Intent Action: {Intent?.Action}");
        System.Diagnostics.Debug.WriteLine($"Intent Data: {Intent?.Data}");
        System.Diagnostics.Debug.WriteLine($"Intent DataString: {Intent?.DataString}");

        base.OnCreate(savedInstanceState);

        // Stel de statusbalk kleur in - gebruik Color.ParseColor direct
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && Window != null)
        {
            // Converteer hex kleur naar Android Color
            var color = global::Android.Graphics.Color.ParseColor("#34495E");
            Window.SetStatusBarColor(color);
        }
    }

    protected override void OnResume()
    {
        System.Diagnostics.Debug.WriteLine("=== MAINACTIVITY ONRESUME ===");
        System.Diagnostics.Debug.WriteLine($"Intent Action: {Intent?.Action}");
        System.Diagnostics.Debug.WriteLine($"Intent Data: {Intent?.Data}");
        System.Diagnostics.Debug.WriteLine($"Intent DataString: {Intent?.DataString}");
        base.OnResume();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        System.Diagnostics.Debug.WriteLine("=== MAINACTIVITY ONNEWINTENT ===");
        System.Diagnostics.Debug.WriteLine($"Intent Action: {intent?.Action}");
        System.Diagnostics.Debug.WriteLine($"Intent Data: {intent?.Data}");
        System.Diagnostics.Debug.WriteLine($"Intent DataString: {intent?.DataString}");
        base.OnNewIntent(intent);
    }
}