using TaskControl.Models;
using static TaskControl.UnitTests.ModelValidation;

namespace TaskControl.UnitTests
{
    public class TaskModelTests
    {
        private static TaskRegistrationModel ValidTask() => new TaskRegistrationModel
        {
            Name = "Write tests",
            Date = "2026-10-04",
            Hour = "09:30",
            Description = "Cover the error cases",
            Priority = TaskPriority.HIGH
        };

        [Fact]
        public void ValidTask_HasNoErrors()
        {
            Assert.Empty(Errors(ValidTask()));
        }

        // TASK_TB.DESCRIPTION is NVARCHAR(100). The form used to allow 150, so 101 to 150
        // characters passed validation and then failed in SQL Server.
        [Fact]
        public void Description_UpToTheColumnSize_IsValid()
        {
            var task = ValidTask();
            task.Description = new string('d', 100);

            Assert.Empty(Errors(task));
        }

        [Fact]
        public void Description_LongerThanTheColumn_IsRejected()
        {
            var task = ValidTask();
            task.Description = new string('d', 101);

            Assert.Contains("Description should have 100 maximum characters.", Errors(task));
        }

        // TASK_TB.NAME is NVARCHAR(150); the form had no limit
        [Fact]
        public void Name_LongerThanTheColumn_IsRejected()
        {
            var task = ValidTask();
            task.Name = new string('n', 151);

            Assert.Contains("Task name should have 150 maximum characters.", Errors(task));
        }

        [Theory]
        [InlineData(nameof(TaskRegistrationModel.Name), "Inform task name.")]
        [InlineData(nameof(TaskRegistrationModel.Date), "Select task date.")]
        [InlineData(nameof(TaskRegistrationModel.Hour), "Inform task hour.")]
        [InlineData(nameof(TaskRegistrationModel.Description), "Inform task description.")]
        [InlineData(nameof(TaskRegistrationModel.Priority), "Select task priority.")]
        public void MissingField_IsRejected(string field, string message)
        {
            var task = ValidTask();
            typeof(TaskRegistrationModel).GetProperty(field)!.SetValue(task, null);

            Assert.Contains(message, Errors(task));
        }

        [Theory]
        [InlineData("abc")]            // not a date
        [InlineData("2026-02-31")]     // no 31st of February
        [InlineData("2026-13-01")]     // no 13th month
        [InlineData("04/10/2026")]     // not the format <input type="date"> posts
        [InlineData("2026-10-04T10:00")]
        public void Date_NotAValidYyyyMmDd_IsRejected(string date)
        {
            var task = ValidTask();
            task.Date = date;

            Assert.Contains("Select a valid task date.", Errors(task));
        }

        [Theory]
        [InlineData("2024-02-29")]     // leap year
        [InlineData("2026-12-31")]
        public void Date_ValidCalendarDays_AreAccepted(string date)
        {
            var task = ValidTask();
            task.Date = date;

            Assert.Empty(Errors(task));
        }

        [Theory]
        [InlineData("24:00")]
        [InlineData("09:60")]
        [InlineData("9h30")]
        [InlineData("noon")]
        public void Hour_NotAValidTimeOfDay_IsRejected(string hour)
        {
            var task = ValidTask();
            task.Hour = hour;

            Assert.Contains("Inform a valid task hour.", Errors(task));
        }

        [Theory]
        [InlineData("00:00")]
        [InlineData("23:59")]
        [InlineData("14:15:30")]       // with seconds
        public void Hour_ValidTimes_AreAccepted(string hour)
        {
            var task = ValidTask();
            task.Hour = hour;

            Assert.Empty(Errors(task));
        }

        [Fact]
        public void EditForm_HasTheSameDescriptionLimit()
        {
            var task = new TaskEditModel
            {
                TaskID = Guid.NewGuid(),
                Name = "Write tests",
                Date = "2026-10-04",
                Hour = "09:30",
                Description = new string('d', 101),
                Priority = TaskPriority.LOW
            };

            Assert.Contains("Description should have 100 maximum characters.", Errors(task));
        }

        [Fact]
        public void Report_FinishBeforeStart_IsRejected()
        {
            var report = new TaskReportModel { StartDate = "2026-10-31", FinishDate = "2026-10-01" };

            Assert.Contains("The finish date cannot be before the start date", Errors(report));
        }

        [Theory]
        [InlineData("2026-10-01", "2026-10-31")]
        [InlineData("2026-10-01", "2026-10-01")]   // a single day
        public void Report_ValidPeriod_IsAccepted(string start, string finish)
        {
            Assert.Empty(Errors(new TaskReportModel { StartDate = start, FinishDate = finish }));
        }

        [Fact]
        public void Report_InvalidDate_IsRejected()
        {
            var errors = Errors(new TaskReportModel { StartDate = "yesterday", FinishDate = "2026-10-01" });

            Assert.Contains("Please inform a valid start date", errors);
        }
    }
}
