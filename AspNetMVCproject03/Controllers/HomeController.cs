using AspNetMVCproject03.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Controllers
{
    [Authorize] //Just allow authenticated users
    public class HomeController : Controller
    {

        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public HomeController(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            try
            {
                var FirstDayOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                var LastDayOfCurrentMonth = FirstDayOfCurrentMonth.AddMonths(1).AddDays(-1);
                TempData["FirstDayOfCurrentMonth"] = FirstDayOfCurrentMonth.ToString("MM/dd/yyyy");
                TempData["LastDayOfCurrentMonth"] = LastDayOfCurrentMonth.ToString("MM/dd/yyyy");

                var email = User.Identity.Name;

                var user = _userRepository.Get(email);

                 var tasks= _taskRepository.GetByUserAndPeriod(user.UserID, FirstDayOfCurrentMonth, LastDayOfCurrentMonth);

                TempData["LowPriorityAmount"] = tasks.Count(t => t.Priority.Equals("LOW"));
                TempData["MediumPriorityAmount"] = tasks.Count(t => t.Priority.Equals("MEDIUM"));
                TempData["HighPriorityAmount"] = tasks.Count(t => t.Priority.Equals("HIGH"));

                TempData["Tasks"] = tasks;
            }
            catch (Exception e)
            {

                TempData["Message"] = e.Message;
            }
            return View();
        }
    }
}
