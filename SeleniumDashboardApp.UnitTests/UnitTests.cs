using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SeleniumDashboard.Shared;
using SeleniumDashboardApp.UnitTests.Models;

namespace SeleniumDashboardApp.UnitTests
{
    [TestFixture]
    public class SimpleUnitTests
    {
        [Test]
        public void Test1_LocalTestRun_PropertiesWork()
        {
            // Arrange & Act
            var testRun = new LocalTestRun
            {
                BackendId = 123,
                ProjectName = "My Test Project",
                Status = "Passed",
                Date = new DateTime(2025, 6, 19, 10, 30, 0),
                Summary = "Unit test summary",
                LogOutput = "✔ Test completed successfully (100ms)"
            };

            // Assert
            Assert.That(testRun.BackendId, Is.EqualTo(123));
            Assert.That(testRun.ProjectName, Is.EqualTo("My Test Project"));
            Assert.That(testRun.Status, Is.EqualTo("Passed"));
            Assert.That(testRun.Date.Year, Is.EqualTo(2025));
            Assert.That(testRun.Summary, Is.EqualTo("Unit test summary"));
            Assert.That(testRun.LogOutput, Contains.Substring("completed successfully"));

            Console.WriteLine("✅ LocalTestRun properties test passed");
        }

        [Test]
        public void Test2_FilterLogic_ByStatus()
        {
            var testRuns = new List<LocalTestRun>
            {
                new LocalTestRun { BackendId = 1, ProjectName = "App Tests", Status = "Passed" },
                new LocalTestRun { BackendId = 2, ProjectName = "Web Tests", Status = "Failed" },
                new LocalTestRun { BackendId = 3, ProjectName = "API Tests", Status = "Passed" },
                new LocalTestRun { BackendId = 4, ProjectName = "UI Tests", Status = "Failed" }
            };

            var passedRuns = testRuns.Where(r => r.Status == "Passed").ToList();
            var failedRuns = testRuns.Where(r => r.Status == "Failed").ToList();

            Assert.That(passedRuns.Count, Is.EqualTo(2), "Should have 2 passed tests");
            Assert.That(failedRuns.Count, Is.EqualTo(2), "Should have 2 failed tests");
            Assert.That(passedRuns.All(r => r.Status == "Passed"), Is.True, "All filtered should be Passed");
            Assert.That(failedRuns.All(r => r.Status == "Failed"), Is.True, "All filtered should be Failed");

            Console.WriteLine("✅ Status filter logic test passed");
        }

        [Test]
        public void Test3_LogParsing_ForCharts()
        {
            var logOutput = @"🚀 Test execution started
                            ✔ Setup completed (25ms)
                            ✔ Login functionality test (150ms)
                            × Navigation test failed (80ms)
                            ✔ Data validation test (45ms)
                            × Form submission failed (120ms)
                            ✅ Test execution completed";

            var lines = logOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var passedCount = lines.Count(l => l.TrimStart().StartsWith("✔"));
            var failedCount = lines.Count(l => l.TrimStart().StartsWith("×"));
            var timedTests = lines.Count(l => l.Contains("ms)"));

            var timings = new List<int>();
            foreach (var line in lines.Where(l => l.Contains("ms)")))
            {
                var startIndex = line.LastIndexOf('(') + 1;
                var endIndex = line.LastIndexOf("ms)");
                if (startIndex > 0 && endIndex > startIndex)
                {
                    var timeStr = line.Substring(startIndex, endIndex - startIndex);
                    if (int.TryParse(timeStr, out int time))
                    {
                        timings.Add(time);
                    }
                }
            }
            Assert.That(passedCount, Is.EqualTo(3), "Should detect 3 passed tests");
            Assert.That(failedCount, Is.EqualTo(2), "Should detect 2 failed tests");
            Assert.That(timedTests, Is.EqualTo(5), "Should detect 5 timed operations");
            Assert.That(timings.Count, Is.EqualTo(5), "Should extract 5 timing values");
            Assert.That(timings.Sum(), Is.EqualTo(420), "Total time should be 420ms (25+150+80+45+120)");
            Assert.That(timings.Max(), Is.EqualTo(150), "Longest operation should be 150ms");
            Assert.That(timings.Min(), Is.EqualTo(25), "Shortest operation should be 25ms");
        }
    }
}