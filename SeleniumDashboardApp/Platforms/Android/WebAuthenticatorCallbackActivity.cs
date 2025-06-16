using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SeleniumDashboardApp.Platforms.Android;

// Simpele callback activity
[Activity(
    NoHistory = true,
    LaunchMode = LaunchMode.SingleTop,
    Exported = true,
    Name = "seleniumdashboardapp.platforms.android.WebAuthenticatorCallbackActivity")]
public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        System.Diagnostics.Debug.WriteLine("=== CALLBACK ACTIVITY ONCREATE ===");
        System.Diagnostics.Debug.WriteLine($"Intent: {Intent}");
        System.Diagnostics.Debug.WriteLine($"Intent Data: {Intent?.Data}");
        System.Diagnostics.Debug.WriteLine($"Intent DataString: {Intent?.DataString}");

        base.OnCreate(savedInstanceState);
        System.Diagnostics.Debug.WriteLine("=== CALLBACK ACTIVITY COMPLETED ===");
    }
}