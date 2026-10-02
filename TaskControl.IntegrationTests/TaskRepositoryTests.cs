using TaskControl.Data.Entities;
using TaskControl.Data.Repository;
using TaskEntity = TaskControl.Data.Entities.Task;

namespace TaskControl.IntegrationTests
{
    [Collection(SqlServerCollection.Name)]
    public class TaskRepositoryTests
    {
        private readonly UserRepository _userRepository;
        private readonly TaskRepository _taskRepository;

        public TaskRepositoryTests(SqlServerFixture fixture)
        {
            _userRepository = new UserRepository(fixture.ConnectionString);
            _taskRepository = new TaskRepository(fixture.ConnectionString);
        }

        // Tasks belong to a user, so each test starts from a fresh one.
        private Guid CreateUser()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            _userRepository.Create(new User { Name = "Task Owner", Email = email, PassWord = "Senha@123" });
            return _userRepository.Get(email).UserID;
        }

        private void CreateTask(Guid userId, string name, DateTime date, TimeSpan hour, string priority = "HIGH") =>
            _taskRepository.Create(new TaskEntity
            {
                Name = name,
                Date = date,
                Hour = hour,
                Description = $"{name} description",
                Priority = priority,
                UserID = userId
            });

        [Fact]
        public void Create_ThenGetByUser_ReturnsAllFields()
        {
            var userId = CreateUser();
            CreateTask(userId, "Write tests", new DateTime(2026, 10, 1), new TimeSpan(9, 30, 0), "MEDIUM");

            var task = Assert.Single(_taskRepository.GetByUser(userId));

            Assert.NotEqual(Guid.Empty, task.TaskID);
            Assert.Equal("Write tests", task.Name);
            Assert.Equal(new DateTime(2026, 10, 1), task.Date);
            Assert.Equal(new TimeSpan(9, 30, 0), task.Hour);
            Assert.Equal("Write tests description", task.Description);
            Assert.Equal("MEDIUM", task.Priority);
            Assert.Equal(userId, task.UserID);
        }

        [Fact]
        public void GetByUser_OnlyReturnsThatUsersTasks()
        {
            var userId = CreateUser();
            var otherUserId = CreateUser();
            CreateTask(userId, "Mine", new DateTime(2026, 10, 1), new TimeSpan(9, 0, 0));
            CreateTask(otherUserId, "Not mine", new DateTime(2026, 10, 1), new TimeSpan(9, 0, 0));

            var task = Assert.Single(_taskRepository.GetByUser(userId));

            Assert.Equal("Mine", task.Name);
        }

        [Fact]
        public void GetByUser_OrdersByDateThenLatestHourFirst()
        {
            var userId = CreateUser();
            CreateTask(userId, "Day 2", new DateTime(2026, 10, 2), new TimeSpan(8, 0, 0));
            CreateTask(userId, "Day 1 morning", new DateTime(2026, 10, 1), new TimeSpan(8, 0, 0));
            CreateTask(userId, "Day 1 evening", new DateTime(2026, 10, 1), new TimeSpan(20, 0, 0));

            var names = _taskRepository.GetByUser(userId).Select(t => t.Name);

            Assert.Equal(new[] { "Day 1 evening", "Day 1 morning", "Day 2" }, names);
        }

        [Fact]
        public void GetByUserAndPeriod_IncludesBothEndsOfThePeriod()
        {
            var userId = CreateUser();
            CreateTask(userId, "Before", new DateTime(2026, 9, 30), new TimeSpan(9, 0, 0));
            CreateTask(userId, "First day", new DateTime(2026, 10, 1), new TimeSpan(9, 0, 0));
            CreateTask(userId, "Last day", new DateTime(2026, 10, 31), new TimeSpan(9, 0, 0));
            CreateTask(userId, "After", new DateTime(2026, 11, 1), new TimeSpan(9, 0, 0));

            var names = _taskRepository
                .GetByUserAndPeriod(userId, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31))
                .Select(t => t.Name);

            Assert.Equal(new[] { "First day", "Last day" }, names);
        }

        [Fact]
        public void Update_ChangesTheTask()
        {
            var userId = CreateUser();
            CreateTask(userId, "Draft", new DateTime(2026, 10, 1), new TimeSpan(9, 0, 0), "LOW");
            var task = _taskRepository.GetByUser(userId).Single();

            task.Name = "Final";
            task.Date = new DateTime(2026, 10, 5);
            task.Hour = new TimeSpan(14, 15, 0);
            task.Description = "Updated description";
            task.Priority = "HIGH";
            _taskRepository.Update(task);

            var updated = _taskRepository.GetTaskById(task.TaskID);
            Assert.Equal("Final", updated.Name);
            Assert.Equal(new DateTime(2026, 10, 5), updated.Date);
            Assert.Equal(new TimeSpan(14, 15, 0), updated.Hour);
            Assert.Equal("Updated description", updated.Description);
            Assert.Equal("HIGH", updated.Priority);
        }

        [Fact]
        public void Delete_RemovesOnlyThatTask()
        {
            var userId = CreateUser();
            CreateTask(userId, "Keep", new DateTime(2026, 10, 1), new TimeSpan(9, 0, 0));
            CreateTask(userId, "Remove", new DateTime(2026, 10, 2), new TimeSpan(9, 0, 0));
            var toRemove = _taskRepository.GetByUser(userId).Single(t => t.Name == "Remove");

            _taskRepository.Delete(toRemove);

            Assert.Null(_taskRepository.GetTaskById(toRemove.TaskID));
            Assert.Equal("Keep", Assert.Single(_taskRepository.GetByUser(userId)).Name);
        }
    }
}
