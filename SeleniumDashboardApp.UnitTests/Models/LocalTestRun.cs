namespace SeleniumDashboardApp.UnitTests.Models
{
    public class LocalTestRun
    {
        public int Id { get; set; }
        public int BackendId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? LogOutput { get; set; }
    }
}