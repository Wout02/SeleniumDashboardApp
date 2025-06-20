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

            if (!string.IsNullOrEmpty(error))
            {
                var errorUrl = $"mauiapp://callback?error={Uri.EscapeDataString(error)}";
                return Redirect(errorUrl);
            }

            if (string.IsNullOrEmpty(code))
            {
                return Redirect("mauiapp://callback?error=no_code");
            }

            var callbackUrl = $"mauiapp://callback?code={Uri.EscapeDataString(code)}";
            if (!string.IsNullOrEmpty(state))
            {
                callbackUrl += $"&state={Uri.EscapeDataString(state)}";
            }

            return Redirect(callbackUrl);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Auth controller is working!", timestamp = DateTime.Now });
        }
    }
}