using Microsoft.AspNetCore.Mvc;

namespace SeleniumDashboardApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        [HttpGet("callback")]
        public IActionResult Callback(string? code, string? state, string? error, string? error_description)
        {
            // Log alle parameters voor debugging
            Console.WriteLine("=== AUTH CALLBACK ONTVANGEN ===");
            Console.WriteLine($"Code: {(string.IsNullOrEmpty(code) ? "null" : code.Substring(0, Math.Min(10, code.Length)) + "...")}");
            Console.WriteLine($"State: {state}");
            Console.WriteLine($"Error: {error}");
            Console.WriteLine($"Error Description: {error_description}");

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Auth error: {error} - {error_description}");
                // Redirect naar app met error
                var errorUrl = $"mauiapp://callback?error={Uri.EscapeDataString(error)}";
                Console.WriteLine($"Redirecting to error URL: {errorUrl}");
                return Redirect(errorUrl);
            }

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("No authorization code received");
                return Redirect("mauiapp://callback?error=no_code");
            }

            Console.WriteLine($"Auth callback successful - redirecting to app with code");

            // BELANGRIJK: Redirect naar de app met de authorization code
            var callbackUrl = $"mauiapp://callback?code={Uri.EscapeDataString(code)}";
            if (!string.IsNullOrEmpty(state))
            {
                callbackUrl += $"&state={Uri.EscapeDataString(state)}";
            }

            Console.WriteLine($"Redirecting to: {callbackUrl}");
            return Redirect(callbackUrl);
        }

        // Optioneel: test endpoint om te checken of de controller werkt
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Auth controller is working!", timestamp = DateTime.Now });
        }
    }
}