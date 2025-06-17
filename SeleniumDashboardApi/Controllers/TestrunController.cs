using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeleniumDashboard.Shared;
using SeleniumDashboardApi.Data;
using SeleniumDashboardApi.Services;

namespace SeleniumDashboardApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestrunController : ControllerBase
    {
        private readonly TestRunDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<TestrunController> _logger;

        public TestrunController(TestRunDbContext context, INotificationService notificationService, ILogger<TestrunController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
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
            try
            {
                _logger.LogInformation($"Creating new test run: {run.ProjectName}");

                _context.TestRuns.Add(run);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Test run saved to database with ID: {run.Id}");

                // Send SignalR notification for new test run
                await _notificationService.NotifyNewTestRun(new
                {
                    id = run.Id.ToString(),
                    projectName = run.ProjectName,
                    status = run.Status,
                    date = run.Date,
                    summary = run.Summary
                });

                _logger.LogInformation($"SignalR notification sent for new test run: {run.Id}");

                return Ok(new { message = "Testrun opgeslagen in database", testRun = run });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test run");
                return StatusCode(500, new { message = "Error creating test run", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestRun(int id, [FromBody] TestRun updatedRun)
        {
            try
            {
                var existingRun = await _context.TestRuns.FindAsync(id);
                if (existingRun == null)
                    return NotFound();

                // Update properties
                existingRun.Status = updatedRun.Status;
                existingRun.Summary = updatedRun.Summary;
                existingRun.LogOutput = updatedRun.LogOutput;
                existingRun.ProjectName = updatedRun.ProjectName;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Test run {id} updated");

                // Send SignalR notification for test run update
                await _notificationService.NotifyTestRunUpdate(id.ToString(), new
                {
                    status = existingRun.Status,
                    progress = CalculateProgress(existingRun),
                    summary = existingRun.Summary,
                    date = existingRun.Date
                });

                // If test run is completed, send completion notification
                if (existingRun.Status == "Completed" || existingRun.Status == "Failed")
                {
                    await _notificationService.NotifyTestRunCompleted(id.ToString(), new
                    {
                        success = existingRun.Status == "Completed",
                        summary = existingRun.Summary,
                        projectName = existingRun.ProjectName,
                        date = existingRun.Date
                    });
                }

                return Ok(existingRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating test run {id}");
                return StatusCode(500, new { message = "Error updating test run", error = ex.Message });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] string? status, [FromQuery] string? project)
        {
            var query = _context.TestRuns.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrEmpty(project))
                query = query.Where(t => t.ProjectName.ToLower().Contains(project.ToLower()));

            return Ok(await query.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var run = await _context.TestRuns.FindAsync(id);
            if (run == null) return NotFound();
            return Ok(run);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestRun(int id)
        {
            try
            {
                var run = await _context.TestRuns.FindAsync(id);
                if (run == null)
                    return NotFound();

                _context.TestRuns.Remove(run);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Test run {id} deleted");

                // Send SignalR notification for test run deletion
                await _notificationService.NotifyTestRunUpdate(id.ToString(), new
                {
                    status = "Deleted",
                    progress = 100,
                    deleted = true
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting test run {id}");
                return StatusCode(500, new { message = "Error deleting test run", error = ex.Message });
            }
        }

        private static int CalculateProgress(TestRun testRun)
        {
            return testRun.Status switch
            {
                "Running" => 50,
                "Completed" => 100,
                "Failed" => 100,
                "Pending" => 0,
                _ => 0
            };
        }
    }
}