using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskControl.Controllers;
using TaskControl.Data.Entities;
using TaskControl.Data.Interfaces;
using TaskControl.Messeges;
using TaskControl.Models;
using TaskControl.Security;

namespace TaskControl.UnitTests
{
    public class PasswordRecoveryTests
    {
        private const string AppUrl = "https://tasks.example.com";

        private readonly Mock<IUserRepository> _users = new();
        private readonly Mock<IPasswordResetRepository> _resets = new();
        private readonly Mock<IEmailService> _email = new();
        private readonly User _user = new() { UserID = Guid.NewGuid(), Name = "Ana <b>", Email = "ana@test.com" };

        private AccountController Controller()
        {
            _users.Setup(r => r.Get(_user.Email)).Returns(_user);
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["AppUrl"] = AppUrl + "/" })
                .Build();
            var httpContext = new DefaultHttpContext();
            // A forged Host header must not end up in the e-mailed link
            httpContext.Request.Host = new HostString("attacker.example");
            return new AccountController(_users.Object, _resets.Object, _email.Object, NullLogger<AccountController>.Instance, configuration)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        [Fact]
        public void Token_Is43UrlSafeCharactersAndNeverRepeats()
        {
            var tokens = Enumerable.Range(0, 1000).Select(_ => PasswordResetToken.NewToken()).ToList();

            Assert.All(tokens, t => Assert.Matches("^[A-Za-z0-9_-]{43}$", t));
            Assert.Equal(tokens.Count, tokens.Distinct().Count());
        }

        [Fact]
        public void Hash_IsSha256HexAndDependsOnTheToken()
        {
            var token = PasswordResetToken.NewToken();

            Assert.Matches("^[0-9A-F]{64}$", PasswordResetToken.Hash(token));
            Assert.Equal(PasswordResetToken.Hash(token), PasswordResetToken.Hash(token));
            Assert.NotEqual(PasswordResetToken.Hash(token), PasswordResetToken.Hash(token + "x"));
        }

        // Same message for a registered and an unknown e-mail, so the page does not reveal accounts
        [Fact]
        public void Recovery_KnownAndUnknownEmail_ShowTheSameMessage()
        {
            var known = Controller();
            known.Passwordrecovery(new AccountPasswordRecoveryModel { Email = _user.Email });
            var unknown = Controller();
            unknown.Passwordrecovery(new AccountPasswordRecoveryModel { Email = "nobody@test.com" });

            Assert.Equal(known.TempData["Message"], unknown.TempData["Message"]);
            Assert.DoesNotContain(_user.Email, (string)known.TempData["Message"]!);
        }

        [Fact]
        public void Recovery_UnknownEmail_SendsNothingAndStoresNothing()
        {
            Controller().Passwordrecovery(new AccountPasswordRecoveryModel { Email = "nobody@test.com" });

            _email.Verify(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _resets.Verify(r => r.Create(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        // The old flow replaced the password with a random number and e-mailed it
        [Fact]
        public void Recovery_KnownEmail_DoesNotChangeThePassword()
        {
            Controller().Passwordrecovery(new AccountPasswordRecoveryModel { Email = _user.Email });

            _users.Verify(r => r.Update(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            _users.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Recovery_KnownEmail_EmailsALinkWhoseTokenMatchesTheStoredHash()
        {
            string? storedHash = null;
            DateTime expiresAt = default;
            _resets.Setup(r => r.Create(_user.UserID, It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<Guid, string, DateTime>((_, hash, expires) => { storedHash = hash; expiresAt = expires; });
            string? body = null;
            _email.Setup(e => e.SendEmail(_user.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((_, _, b) => body = b);
            var before = DateTime.UtcNow;

            Controller().Passwordrecovery(new AccountPasswordRecoveryModel { Email = _user.Email });

            var link = System.Text.RegularExpressions.Regex.Match(body!, "href='([^']+)'").Groups[1].Value;
            Assert.StartsWith(AppUrl + "/Account/ResetPassword?token=", link);
            Assert.DoesNotContain("attacker.example", body);
            var token = Uri.UnescapeDataString(link.Split("token=")[1]);
            Assert.Equal(PasswordResetToken.Hash(token), storedHash);
            Assert.DoesNotContain(storedHash!, body);
            Assert.InRange(expiresAt, before.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
            Assert.Contains("Ana &lt;b&gt;", body);   // the name is HTML-encoded
        }

        // A mail server failure must not show a different message for registered e-mails
        [Fact]
        public void Recovery_WhenTheEmailFails_ShowsTheSameMessage()
        {
            _email.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("SMTP is down"));
            var failing = Controller();
            failing.Passwordrecovery(new AccountPasswordRecoveryModel { Email = _user.Email });
            var unknown = Controller();
            unknown.Passwordrecovery(new AccountPasswordRecoveryModel { Email = "nobody@test.com" });

            Assert.Equal(unknown.TempData["Message"], failing.TempData["Message"]);
        }

        [Fact]
        public void Reset_ValidToken_SavesTheChosenPasswordForThatUser()
        {
            var token = PasswordResetToken.NewToken();
            _resets.Setup(r => r.Consume(PasswordResetToken.Hash(token), It.IsAny<DateTime>())).Returns(_user.UserID);

            var result = Controller().ResetPassword(new AccountResetPasswordModel { Token = token, NewPassWord = "Nova@1234", NewPassWordConfirm = "Nova@1234" });

            _users.Verify(r => r.Update(_user.UserID, "Nova@1234"), Times.Once);
            Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(result).ActionName);
        }

        [Fact]
        public void Reset_InvalidExpiredOrUsedToken_ChangesNothing()
        {
            _resets.Setup(r => r.Consume(It.IsAny<string>(), It.IsAny<DateTime>())).Returns((Guid?)null);

            var result = Controller().ResetPassword(new AccountResetPasswordModel { Token = "made-up", NewPassWord = "Nova@1234", NewPassWordConfirm = "Nova@1234" });

            _users.Verify(r => r.Update(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
            Assert.Equal("Passwordrecovery", Assert.IsType<RedirectToActionResult>(result).ActionName);
        }

        [Fact]
        public void Reset_InvalidForm_DoesNotUseTheToken()
        {
            var controller = Controller();
            controller.ModelState.AddModelError("NewPassWord", "weak");

            controller.ResetPassword(new AccountResetPasswordModel { Token = PasswordResetToken.NewToken(), NewPassWord = "weak", NewPassWordConfirm = "weak" });

            _resets.Verify(r => r.Consume(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Theory]
        [InlineData("abc", "Nova@1234", "Nova@1234", 0)]
        [InlineData("abc", "fraca", "fraca", 2)]               // too short and not strong
        [InlineData("abc", "Nova@1234", "Outra@1234", 1)]      // confirmation differs
        [InlineData(null, "Nova@1234", "Nova@1234", 1)]        // the link had no token
        public void ResetModel_Validation(string? token, string password, string confirmation, int errors)
        {
            var model = new AccountResetPasswordModel { Token = token!, NewPassWord = password, NewPassWordConfirm = confirmation };

            Assert.Equal(errors, ModelValidation.Errors(model).Count);
        }
    }
}
