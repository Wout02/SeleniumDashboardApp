using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeleniumDashboard.Shared;
using SeleniumDashboardApi.Data;

namespace SeleniumDashboardApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestrunController : ControllerBase
    {
        private readonly TestRunDbContext _context;

        public TestrunController(TestRunDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var all = await _context.TestRuns.ToListAsync();
            return Ok(all);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TestRun run)
        {
            _context.TestRuns.Add(run);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Testrun opgeslagen in database" });
        }
    }
}
