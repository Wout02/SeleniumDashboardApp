using SQLite;
using SeleniumDashboard.Shared;

namespace SeleniumDashboardApp.Models
{
    public class LocalTestRun
    {
        [PrimaryKey, AutoIncrement]
        public int LocalId { get; set; }

        public int BackendId { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public string? LogOutput { get; set; }
    }
}