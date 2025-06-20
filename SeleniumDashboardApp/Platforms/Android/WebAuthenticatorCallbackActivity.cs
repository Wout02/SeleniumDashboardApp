using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SeleniumDashboardApp.Platforms.Android;

[Activity(
    NoHistory = true,
    LaunchMode = LaunchMode.SingleTop,
    Exported = true,
    Name = "seleniumdashboardapp.platforms.android.WebAuthenticatorCallbackActivity")]
public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
}