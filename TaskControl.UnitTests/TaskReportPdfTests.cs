using System.Text;
using TaskControl.Data.Entities;
using TaskControl.Reports.Data;
using TaskControl.Reports.Pdfs;
using TaskEntity = TaskControl.Data.Entities.Task;

namespace TaskControl.UnitTests
{
    public class TaskReportPdfTests
    {
        private static TaskReportData Report(params TaskEntity[] tasks) => new TaskReportData
        {
            StartDate = new DateTime(2026, 11, 1),
            FinishDate = new DateTime(2026, 11, 30),
            User = new User { Name = "Ana Martins", Email = "ana@taskcontrol.dev" },
            Tasks = tasks.ToList()
        };

        private static TaskEntity Task(string name) => new TaskEntity
        {
            Name = name,
            Date = new DateTime(2026, 11, 12),
            Hour = new TimeSpan(10, 0, 0),
            Description = "Description",
            Priority = "HIGH"
        };

        // The report used to fetch a logo from a third-party site, which answered 403, so no PDF was ever made
        [Fact]
        public void Report_IsAPdfBuiltWithoutTheNetwork()
        {
            var pdf = new TaskReportPdf().ReportGenerator(Report(Task("Sprint planning"), Task("Dentist")));

            Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
            Assert.True(pdf.Length > 1000);
        }

        [Fact]
        public void Report_WithNoTasksInThePeriod_IsStillAValidPdf()
        {
            var pdf = new TaskReportPdf().ReportGenerator(Report());

            Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
        }
    }
}
