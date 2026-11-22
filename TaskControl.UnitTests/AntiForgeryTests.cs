using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaskControl.Controllers;

namespace TaskControl.UnitTests
{
    // Another site can make a browser submit a form with the user's cookie; the anti-forgery token is
    // what it cannot copy. It used to be checked on the delete action only.
    public class AntiForgeryTests
    {
        [Fact]
        public void EveryPostIsCheckedForAnAntiForgeryToken()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            new Startup(new ConfigurationBuilder().Build()).ConfigureServices(services);

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<MvcOptions>>().Value;

            Assert.Contains(options.Filters, filter => filter is AutoValidateAntiforgeryTokenAttribute);
        }

        // The filter skips GET, so a state-changing action reachable by GET would escape it
        [Fact]
        public void LogOut_OnlyAcceptsPost()
        {
            var logout = typeof(AccountController).GetMethod(nameof(AccountController.Logout))!;

            Assert.NotNull(logout.GetCustomAttribute<HttpPostAttribute>());
            Assert.Null(logout.GetCustomAttribute<HttpGetAttribute>());
        }
    }
}
