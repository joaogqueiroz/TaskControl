using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Controllers
{
    public class TaskController : Controller
    {
        [Authorize]
        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Consult()
        {
            return View();
        }
        
        public IActionResult Report()
        {
            return View();
        }
    }
}
