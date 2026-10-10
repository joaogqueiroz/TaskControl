using TaskControl.Data.Entities;
using TaskControl.Data.Repository;

namespace TaskControl.IntegrationTests
{
    [Collection(SqlServerCollection.Name)]
    public class PasswordResetRepositoryTests
    {
        private readonly UserRepository _users;
        private readonly PasswordResetRepository _resets;

        public PasswordResetRepositoryTests(SqlServerFixture fixture)
        {
            _users = new UserRepository(fixture.ConnectionString);
            _resets = new PasswordResetRepository(fixture.ConnectionString);
        }

        private Guid CreateUser()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            _users.Create(new User { Name = "Test User", Email = email, PassWord = "Senha@123" });
            return _users.Get(email).UserID;
        }

        private static string NewHash() => Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        private static readonly DateTime Now = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Consume_ValidToken_ReturnsItsUser()
        {
            var userId = CreateUser();
            var hash = NewHash();
            _resets.Create(userId, hash, Now.AddMinutes(30));

            Assert.Equal(userId, _resets.Consume(hash, Now));
        }

        [Fact]
        public void Consume_SecondTime_ReturnsNull()
        {
            var userId = CreateUser();
            var hash = NewHash();
            _resets.Create(userId, hash, Now.AddMinutes(30));

            _resets.Consume(hash, Now);

            Assert.Null(_resets.Consume(hash, Now.AddMinutes(1)));
        }

        [Fact]
        public void Consume_AfterExpiry_ReturnsNull()
        {
            var userId = CreateUser();
            var hash = NewHash();
            _resets.Create(userId, hash, Now.AddMinutes(30));

            Assert.Null(_resets.Consume(hash, Now.AddMinutes(30)));
        }

        [Fact]
        public void Consume_UnknownToken_ReturnsNull()
        {
            Assert.Null(_resets.Consume(NewHash(), Now));
        }

        // Asking for a new link makes the previous one stop working
        [Fact]
        public void Create_NewToken_DiscardsTheUnusedOne()
        {
            var userId = CreateUser();
            var first = NewHash();
            var second = NewHash();
            _resets.Create(userId, first, Now.AddMinutes(30));
            _resets.Create(userId, second, Now.AddMinutes(30));

            Assert.Null(_resets.Consume(first, Now));
            Assert.Equal(userId, _resets.Consume(second, Now));
        }

        [Fact]
        public void Create_ForAUserThatDoesNotExist_Throws()
        {
            Assert.ThrowsAny<Exception>(() => _resets.Create(Guid.NewGuid(), NewHash(), Now.AddMinutes(30)));
        }

        // The whole flow at the database level: the new password works and the old one does not
        [Fact]
        public void ConsumeThenUpdate_ChangesThePassword()
        {
            var email = $"{Guid.NewGuid():N}@test.com";
            _users.Create(new User { Name = "Test User", Email = email, PassWord = "Senha@123" });
            var hash = NewHash();
            _resets.Create(_users.Get(email).UserID, hash, Now.AddMinutes(30));

            _users.Update(_resets.Consume(hash, Now)!.Value, "Nova@1234");

            Assert.NotNull(_users.Get(email, "Nova@1234"));
            Assert.Null(_users.Get(email, "Senha@123"));
        }
    }
}
