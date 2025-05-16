using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeleniumDashboard.Shared
{
    public class TestRun
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ProjectName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
    }
}