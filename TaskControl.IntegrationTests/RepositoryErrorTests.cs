using System.Data.SqlClient;
using TaskControl.Data.Entities;
using TaskControl.Data.Repository;
using TaskEntity = TaskControl.Data.Entities.Task;

namespace TaskControl.IntegrationTests
{
    // What the database does with data the forms are supposed to stop, and with ids that do not exist.
    [Collection(SqlServerCollection.Name)]
    public class RepositoryErrorTests
    {
        private readonly UserRepository _userRepository;
        private readonly TaskRepository _taskRepository;

        public RepositoryErrorTests(SqlServerFixture fixture)
        {
            _userRepository = new UserRepository(fixture.ConnectionString);
            _taskRepository = new TaskRepository(fixture.ConnectionString);
        }

        private User CreateUser()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            _userRepository.Create(new User { Name = "Test User", Email = email, PassWord = "Senha@123" });
            return _userRepository.Get(email);
        }

        private static TaskEntity Task(Guid userId, string name = "Task", string description = "Description") => new TaskEntity
        {
            Name = name,
            Date = new DateTime(2026, 10, 4),
            Hour = new TimeSpan(9, 0, 0),
            Description = description,
            Priority = "HIGH",
            UserID = userId
        };

        [Fact]
        public void CreateUser_DuplicateEmail_IsRejectedByTheUniqueConstraint()
        {
            var user = CreateUser();

            Assert.ThrowsAny<SqlException>(() =>
                _userRepository.Create(new User { Name = "Someone Else", Email = user.Email, PassWord = "Outra@123" }));
        }

        [Fact]
        public void CreateUser_EmailLongerThanTheColumn_IsRejected()
        {
            var email = new string('a', 92) + "@test.com";   // 101 characters, the form now stops this

            Assert.ThrowsAny<SqlException>(() =>
                _userRepository.Create(new User { Name = "Test User", Email = email, PassWord = "Senha@123" }));
        }

        [Fact]
        public void CreateTask_ForAUserThatDoesNotExist_IsRejectedByTheForeignKey()
        {
            Assert.ThrowsAny<SqlException>(() => _taskRepository.Create(Task(Guid.NewGuid())));
        }

        [Fact]
        public void CreateTask_DescriptionLongerThanTheColumn_IsRejected()
        {
            var user = CreateUser();

            Assert.ThrowsAny<SqlException>(() => _taskRepository.Create(Task(user.UserID, description: new string('d', 101))));
            Assert.Empty(_taskRepository.GetByUser(user.UserID));
        }

        [Fact]
        public void CreateTask_NameLongerThanTheColumn_IsRejected()
        {
            var user = CreateUser();

            Assert.ThrowsAny<SqlException>(() => _taskRepository.Create(Task(user.UserID, name: new string('n', 151))));
        }

        [Fact]
        public void CreateTask_ValuesExactlyAtTheColumnLimits_AreStored()
        {
            var user = CreateUser();

            _taskRepository.Create(Task(user.UserID, name: new string('n', 150), description: new string('d', 100)));

            var task = Assert.Single(_taskRepository.GetByUser(user.UserID));
            Assert.Equal(150, task.Name.Length);
            Assert.Equal(100, task.Description.Length);
        }

        [Fact]
        public void Reads_WithUnknownIds_ReturnNull()
        {
            Assert.Null(_userRepository.GetById(Guid.NewGuid()));
            Assert.Null(_userRepository.Get($"{Guid.NewGuid():N}@test.com"));
            Assert.Null(_taskRepository.GetTaskById(Guid.NewGuid()));
        }

        [Fact]
        public void GetByUser_UserWithoutTasks_ReturnsAnEmptyList()
        {
            var user = CreateUser();

            Assert.Empty(_taskRepository.GetByUser(user.UserID));
            Assert.Empty(_taskRepository.GetByUser(Guid.NewGuid()));
        }

        [Fact]
        public void UpdateTask_ThatDoesNotExist_ChangesNothing()
        {
            var user = CreateUser();
            _taskRepository.Create(Task(user.UserID, name: "Existing"));

            var ghost = Task(user.UserID, name: "Ghost");
            ghost.TaskID = Guid.NewGuid();
            _taskRepository.Update(ghost);

            Assert.Equal("Existing", Assert.Single(_taskRepository.GetByUser(user.UserID)).Name);
        }

        [Fact]
        public void DeleteTask_ThatDoesNotExist_RemovesNothing()
        {
            var user = CreateUser();
            _taskRepository.Create(Task(user.UserID));

            var ghost = Task(user.UserID);
            ghost.TaskID = Guid.NewGuid();
            _taskRepository.Delete(ghost);

            Assert.Single(_taskRepository.GetByUser(user.UserID));
        }

        [Fact]
        public void ChangePassword_ForUnknownUser_DoesNotAffectOthers()
        {
            var user = CreateUser();

            _userRepository.Update(Guid.NewGuid(), "Nova@4567");

            Assert.NotNull(_userRepository.Get(user.Email, "Senha@123"));
        }

        [Fact]
        public void GetByUserAndPeriod_FinishBeforeStart_ReturnsNothing()
        {
            var user = CreateUser();
            _taskRepository.Create(Task(user.UserID));

            var tasks = _taskRepository.GetByUserAndPeriod(user.UserID, new DateTime(2026, 10, 31), new DateTime(2026, 10, 1));

            Assert.Empty(tasks);
        }
    }
}
