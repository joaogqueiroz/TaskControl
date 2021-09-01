using AspNetMVCproject03.Data.Interfaces;
using AspNetMVCproject03.Data.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //This command enable MVC
            services.AddControllersWithViews();

            //Using cookies and authentication
            services.Configure<CookiePolicyOptions>(options => { options.MinimumSameSitePolicy = SameSiteMode.None; });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            //DB connectionString
            var connectionString = Configuration.GetConnectionString("DB_context");
            //Dependency injection
            services.AddTransient<IUserRepository, UserRepository>(map => new UserRepository(connectionString));
            services.AddTransient<ITaskRepository, TaskRepository>(map => new TaskRepository(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            app.UseRouting();

            //Using cookies
            app.UseCookiePolicy();
            //Using authentication
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default", //Initial page
                    pattern: "{controller=Account}/{action=Login}");
            });
        }
    }
}
