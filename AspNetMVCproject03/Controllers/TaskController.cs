using AspNetMVCproject03.Data.Entities;
using AspNetMVCproject03.Data.Interfaces;
using AspNetMVCproject03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetMVCproject03.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskController(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        public IActionResult Registration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Registration(TaskRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //getting user email
                    var email = User.Identity.Name;

                    //getting user data
                    var user = _userRepository.Get(email);

                    //creating task
                    var task = new Task();
                    task.Name = model.Name;
                    task.Date = DateTime.Parse(model.Date);
                    task.Hour = TimeSpan.Parse(model.Hour);
                    task.Description = model.Description;
                    task.Priority =  model.Priority.ToString();
                    task.UserID = user.UserID; //Foreign key

                    _taskRepository.Create(task);
                    TempData["Message"] = $"Task created {task.Name} successfully";
                    ModelState.Clear();
                }
                catch (Exception e)
                {

                    TempData["Message"] = "Erro: " + e.Message;
                }
            }
                return View();
        }

        public IActionResult Consult()
        {
            try
            {
                //getting user email
                var email = User.Identity.Name;

                //getting user informations using email
                var user = _userRepository.Get(email);

                //getting user tasks
                TempData["Tasks"] = _taskRepository.GetByUser(user.UserID);

            }
            catch (Exception e)
            {

                TempData["Messege"] = "Error" + e.Message;
            }
            return View();
        }

        [HttpPost]
        public IActionResult Consult(TaskConsultModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                }
                catch (Exception e)
                {

                    TempData["Message"] = "Erro: " + e.Message;
                }
            }
            return View();
        }

        public IActionResult Report()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Report(TaskReportModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                }
                catch (Exception e)
                {

                    TempData["Message"] = "Erro: " + e.Message;
                }
            }
            return View();
        }
    }
}
