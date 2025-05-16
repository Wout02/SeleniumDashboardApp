using Microsoft.EntityFrameworkCore;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApi.Data
{
    public class TestRunDbContext : DbContext
    {
        public TestRunDbContext(DbContextOptions<TestRunDbContext> options) : base(options)
        {
        }

        public DbSet<TestRun> TestRuns { get; set; }
    }
}
