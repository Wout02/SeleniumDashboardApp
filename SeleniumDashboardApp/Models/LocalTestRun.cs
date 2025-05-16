using SQLite;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.Models
{
    public class LocalTestRun : TestRun
    {
        [PrimaryKey, AutoIncrement]
        public new int Id { get; set; }
    }
}
