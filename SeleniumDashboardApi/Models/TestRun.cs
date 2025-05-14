namespace SeleniumDashboardApi.Models
{
    public class TestRun
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
    }
}