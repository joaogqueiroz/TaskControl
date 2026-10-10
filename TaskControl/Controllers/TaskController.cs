using TaskControl.Data.Entities;
using TaskControl.Data.Interfaces;
using TaskControl.Models;
using TaskControl.Reports.Data;
using TaskControl.Reports.Pdfs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskControl.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskController(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

       
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
                    task.Priority = model.Priority.ToString();
                    task.UserID = user.UserID; //Foreign key

                    _taskRepository.Create(task);
                    TempData["Message"] = $"Task {task.Name}, was created successfully";
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

                TempData["Message"] = "Error: " + e.Message;
            }
            return View();
        }
        // The signed-in user, from the e-mail in the auth cookie
        private Guid CurrentUserId() => _userRepository.Get(User.Identity.Name).UserID;

        public IActionResult Edit(Guid id)
        {
            try
            {
                //taking the task, only if it belongs to the signed-in user
                var task = _taskRepository.GetTaskById(id, CurrentUserId());
                if (task == null)
                {
                    TempData["Message"] = "Task not found.";
                    return RedirectToAction("Consult");
                }
                var model = new TaskEditModel();
                model.TaskID = task.TaskID;
                model.Name = task.Name;
                model.Date = task.Date.ToString("yyyy-MM-dd");
                model.Hour = task.Hour.ToString(@"hh\:mm");
                model.Description = task.Description;
                model.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), task.Priority);
                return View(model);
            }
            catch (Exception e)
            {
                TempData["Message"] = e.Message;
            }
            return View();
        }
        [HttpPost]
        public IActionResult Edit(TaskEditModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   //Editing task. The owner comes from the cookie, not from the form, so a
                   //TaskID changed in the form cannot reach another user's task.
                    var task = new Task();
                    task.TaskID = model.TaskID;
                    task.UserID = CurrentUserId();
                    task.Name = model.Name;
                    task.Date = DateTime.Parse(model.Date);
                    task.Hour = TimeSpan.Parse(model.Hour);
                    task.Description = model.Description;
                    task.Priority = model.Priority.ToString();
                    TempData["Message"] = _taskRepository.Update(task)
                        ? $"Task {task.Name}, was updated successfully"
                        : "Task not found.";
                    return RedirectToAction("Consult");
                }
                catch (Exception e)
                {
                TempData["Message"] = e.Message;
                }
            }
            return View();
        }

        // POST with an anti-forgery token: a plain link (GET) could delete a task from any page
        // that tricks the user into opening it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            try
            {
                TempData["Message"] = _taskRepository.Delete(id, CurrentUserId())
                    ? "The task was deleted successfully."
                    : "Task not found.";
            }
            catch (Exception e)
            {
                TempData["Message"] = e.Message;
            }
            return RedirectToAction("Consult");
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
                    //Parsing date from Model

                    var startDate = DateTime.Parse(model.StartDate);
                    var finishDate = DateTime.Parse(model.FinishDate);

                    //getting user email
                    var email = User.Identity.Name;

                    //getting user informations using email
                    var user = _userRepository.Get(email);

                    var tasks = _taskRepository.GetByUserAndPeriod(user.UserID, startDate, finishDate);



                    //creating object to transport report data to report layer
                    var data = new TaskReportData();
                    data.StartDate = startDate;
                    data.FinishDate = finishDate;
                    data.User = user;
                    data.Tasks = tasks;

                    var taskReportPdf = new TaskReportPdf();
                    var pdf = taskReportPdf.ReportGenerator(data);

                    //Downloading file!
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.Headers.Add("content-disposition", "attachment; filename=TaskReport.pdf");
                    Response.Body.WriteAsync(pdf, 0, pdf.Length);
                    Response.Body.Flush();
                    Response.StatusCode = StatusCodes.Status200OK;
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