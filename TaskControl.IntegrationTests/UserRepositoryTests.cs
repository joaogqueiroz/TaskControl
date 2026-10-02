using TaskControl.Data.Entities;
using TaskControl.Data.Repository;

namespace TaskControl.IntegrationTests
{
    [Collection(SqlServerCollection.Name)]
    public class UserRepositoryTests
    {
        private readonly UserRepository _repository;

        public UserRepositoryTests(SqlServerFixture fixture)
        {
            _repository = new UserRepository(fixture.ConnectionString);
        }

        // Every test gets its own e-mail so tests never see each other's rows.
        private User CreateUser(string password = "Senha@123")
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            _repository.Create(new User { Name = "Test User", Email = email, PassWord = password });
            return _repository.Get(email);
        }

        [Fact]
        public void Create_StoresAHashInsteadOfThePassword()
        {
            var user = CreateUser("Senha@123");

            Assert.NotEqual(Guid.Empty, user.UserID);
            Assert.NotEqual("Senha@123", user.PassWord);
            Assert.Equal(84, user.PassWord.Length);
        }

        [Fact]
        public void GetByEmailAndPassword_RightPassword_ReturnsUser()
        {
            var user = CreateUser("Senha@123");

            var loggedIn = _repository.Get(user.Email, "Senha@123");

            Assert.NotNull(loggedIn);
            Assert.Equal(user.UserID, loggedIn.UserID);
        }

        [Fact]
        public void GetByEmailAndPassword_WrongPassword_ReturnsNull()
        {
            var user = CreateUser("Senha@123");

            Assert.Null(_repository.Get(user.Email, "Senha@124"));
        }

        [Fact]
        public void GetByEmailAndPassword_UnknownEmail_ReturnsNull()
        {
            Assert.Null(_repository.Get($"{Guid.NewGuid():N}@test.com", "Senha@123"));
        }

        [Fact]
        public void ChangePassword_OldPasswordStopsWorking()
        {
            var user = CreateUser("Senha@123");

            _repository.Update(user.UserID, "Nova@4567");

            Assert.Null(_repository.Get(user.Email, "Senha@123"));
            Assert.NotNull(_repository.Get(user.Email, "Nova@4567"));
        }

        [Fact]
        public void UpdateUser_ChangesNameEmailAndPassword()
        {
            var user = CreateUser("Senha@123");
            var newEmail = $"{Guid.NewGuid():N}@test.com";

            _repository.Update(new User { UserID = user.UserID, Name = "Renamed", Email = newEmail, PassWord = "Nova@4567" });

            var updated = _repository.GetById(user.UserID);
            Assert.Equal("Renamed", updated.Name);
            Assert.Equal(newEmail, updated.Email);
            Assert.NotNull(_repository.Get(newEmail, "Nova@4567"));
        }

        [Fact]
        public void Delete_RemovesTheUser()
        {
            var user = CreateUser();

            _repository.Delete(user);

            Assert.Null(_repository.GetById(user.UserID));
        }
    }
}
