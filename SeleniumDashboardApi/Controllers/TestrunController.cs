using Microsoft.AspNetCore.Mvc;
using SeleniumDashboardApi.Models;

namespace SeleniumDashboardApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestrunController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var runs = new List<TestRun>
            {
                new TestRun { Id = 1, ProjectName = "App1", Status = "Passed", Date = DateTime.Now.AddDays(-1), Summary = "All tests passed" },
                new TestRun { Id = 2, ProjectName = "App2", Status = "Failed", Date = DateTime.Now, Summary = "Login test failed" }
            };

            return Ok(runs);
        }
    }
}
