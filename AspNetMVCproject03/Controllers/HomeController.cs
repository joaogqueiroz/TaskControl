using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Controllers
{
    public class HomeController : Controller
    {
        [Authorize] //Just allow authenticated users
        public IActionResult Index()
        {
            return View();
        }
    }
}
