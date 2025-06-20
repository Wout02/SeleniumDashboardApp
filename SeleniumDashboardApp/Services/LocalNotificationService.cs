using Microsoft.Extensions.Logging;

#if ANDROID
using AndroidX.Core.App;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using Android.OS;
#endif

namespace SeleniumDashboardApp.Services
{
    public interface ILocalNotificationService
    {
        Task ShowNotificationAsync(string title, string message, string? data = null);
        Task RequestPermissionAsync();
    }

    public class SimpleRealNotificationService : ILocalNotificationService
    {
        private readonly ILogger<SimpleRealNotificationService> _logger;
        private const int NOTIFICATION_ID_BASE = 1000;

        public SimpleRealNotificationService(ILogger<SimpleRealNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task RequestPermissionAsync()
        {
            try
            {
#if ANDROID
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
                {
                    var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                    _logger.LogInformation($"Notification permission status: {status}");
                }
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request notification permission");
            }
        }

        public async Task ShowNotificationAsync(string title, string message, string? data = null)
        {
            try
            {
                _logger.LogInformation($"Showing notification: {title} - {message}");

#if ANDROID
                await ShowAndroidNotificationAsync(title, message, data);
#else
                System.Diagnostics.Debug.WriteLine($"🔔 NOTIFICATION: {title} - {message}");
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show notification");
                System.Diagnostics.Debug.WriteLine($"🔔 NOTIFICATION (error fallback): {title} - {message}");
            }
        }

#if ANDROID
        private async Task ShowAndroidNotificationAsync(string title, string message, string? data = null)
        {
            var context = Platform.CurrentActivity ?? Android.App.Application.Context;
            
            if (context == null)
            {
                _logger.LogWarning("No Android context available for notification");
                return;
            }

            try
            {
                // Create notification channel (required for Android 8.0+)
                var channelId = "selenium_testrun_notifications";
                var channelName = "Selenium Test Run Notifications";
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
                    if (notificationManager != null)
                    {
                        var existingChannel = notificationManager.GetNotificationChannel(channelId);
                        if (existingChannel == null)
                        {
                            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.Default)
                            {
                                Description = "Notifications for Selenium test run updates"
                            };
                            channel.EnableLights(true);
                            channel.EnableVibration(true);
                            notificationManager.CreateNotificationChannel(channel);
                            _logger.LogInformation($"Created notification channel: {channelId}");
                        }
                    }
                }

                // Build notification WITHOUT tap intent (simpler approach)
                var builder = new NotificationCompat.Builder(context, channelId)
                    .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo) // Use built-in icon
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetPriority(NotificationCompat.PriorityDefault)
                    .SetAutoCancel(true)
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetStyle(new NotificationCompat.BigTextStyle().BigText(message));

                var notificationManagerCompat = NotificationManagerCompat.From(context);
                
                if (NotificationManagerCompat.From(context).AreNotificationsEnabled())
                {
                    var notificationId = NOTIFICATION_ID_BASE + new Random().Next(1000);
                    notificationManagerCompat.Notify(notificationId, builder.Build());
                    _logger.LogInformation($"Android notification shown successfully with ID: {notificationId}");
                }
                else
                {
                    _logger.LogWarning("Notifications are disabled for this app");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show Android notification");
                // Fallback to debug output
                System.Diagnostics.Debug.WriteLine($"🔔 NOTIFICATION (fallback): {title} - {message}");
            }

            await Task.CompletedTask;
        }
#endif
    }
}