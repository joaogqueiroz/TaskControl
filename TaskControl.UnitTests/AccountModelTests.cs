using TaskControl.Models;
using static TaskControl.UnitTests.ModelValidation;

namespace TaskControl.UnitTests
{
    public class AccountModelTests
    {
        private const string StrongPasswordMessage =
            "Inform a uppercase letter 1, 1 lowercase letter, 1 number and 1 special character(! @ # $ % & ).";

        private static AccountRegistrationModel ValidRegistration() => new AccountRegistrationModel
        {
            Name = "Test User",
            Email = "user@test.com",
            PassWord = "Senha@123",
            PassWordConfirm = "Senha@123"
        };

        [Fact]
        public void ValidRegistration_HasNoErrors()
        {
            Assert.Empty(Errors(ValidRegistration()));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("user@")]
        [InlineData("@test.com")]
        public void Registration_InvalidEmail_IsRejected(string email)
        {
            var model = ValidRegistration();
            model.Email = email;

            Assert.Contains("Inform a valid email.", Errors(model));
        }

        // USER_TB.EMAIL is NVARCHAR(100); the form had no limit
        [Fact]
        public void Registration_EmailLongerThanTheColumn_IsRejected()
        {
            var model = ValidRegistration();
            model.Email = new string('a', 92) + "@test.com";   // 101 characters

            Assert.Contains("Email should have 100 maximum characters.", Errors(model));
        }

        [Fact]
        public void Registration_NameTooShort_IsRejected()
        {
            var model = ValidRegistration();
            model.Name = "Joao";

            Assert.Contains("Type a minimum of 6 characters", Errors(model));
        }

        [Theory]
        [InlineData("senha@123")]    // no uppercase
        [InlineData("SENHA@123")]    // no lowercase
        [InlineData("Senha@abc")]    // no digit
        [InlineData("Senha1234")]    // no special character
        [InlineData("Senha*123")]    // * is not one of the accepted special characters
        public void Registration_WeakPassword_IsRejected(string password)
        {
            var model = ValidRegistration();
            model.PassWord = password;
            model.PassWordConfirm = password;

            Assert.Contains(StrongPasswordMessage, Errors(model));
        }

        [Theory]
        [InlineData("Se@1", "Type a minimum of 8 characters")]
        [InlineData("Senha@123456789012345", "Type a maximum of 20 characters")]
        public void Registration_PasswordLength_IsChecked(string password, string message)
        {
            var model = ValidRegistration();
            model.PassWord = password;
            model.PassWordConfirm = password;

            Assert.Contains(message, Errors(model));
        }

        [Fact]
        public void Registration_ConfirmationDifferentFromPassword_IsRejected()
        {
            var model = ValidRegistration();
            model.PassWordConfirm = "Senha@124";

            Assert.Contains("The passwords don't are equals", Errors(model));
        }

        [Fact]
        public void StrongPassWord_NullValue_IsNotValid()
        {
            Assert.False(new StrongPassWord().IsValid(null));
        }

        [Fact]
        public void Login_ShortPassword_IsRejected()
        {
            var errors = Errors(new AccountLoginModel { Email = "user@test.com", PassWord = "1234567" });

            Assert.Contains("Password should have 8 minimum characters.", errors);
        }

        [Fact]
        public void PasswordRecovery_InvalidEmail_IsRejected()
        {
            Assert.Contains("Inform a valid email.", Errors(new AccountPasswordRecoveryModel { Email = "nope" }));
        }
    }
}
