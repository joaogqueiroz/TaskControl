using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using TaskControl.Controllers;
using TaskControl.Data.Entities;
using TaskControl.Data.Interfaces;
using TaskControl.Models;
using TaskEntity = TaskControl.Data.Entities.Task;

namespace TaskControl.UnitTests
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskRepository> _tasks = new();
        private readonly Mock<IUserRepository> _users = new();
        private readonly User _signedIn = new() { UserID = Guid.NewGuid(), Email = "ana@test.com" };

        private TaskController Controller()
        {
            _users.Setup(r => r.Get(_signedIn.Email)).Returns(_signedIn);
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, _signedIn.Email) }, "Cookies"))
            };
            return new TaskController(_tasks.Object, _users.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };
        }

        [Fact]
        public void EditPage_SomeoneElsesTask_RedirectsWithoutShowingIt()
        {
            var taskId = Guid.NewGuid();
            _tasks.Setup(r => r.GetTaskById(taskId, _signedIn.UserID)).Returns((TaskEntity?)null);

            var result = Controller().Edit(taskId);

            Assert.Equal("Consult", Assert.IsType<RedirectToActionResult>(result).ActionName);
            _tasks.Verify(r => r.GetTaskById(taskId, _signedIn.UserID), Times.Once);
        }

        // The owner sent to the repository is the signed-in user, whatever TaskID the form posts
        [Fact]
        public void Edit_SendsTheSignedInUserAsTheOwner()
        {
            _tasks.Setup(r => r.Update(It.IsAny<TaskEntity>())).Returns(true);
            var model = new TaskEditModel { TaskID = Guid.NewGuid(), Name = "Task", Date = "2026-10-10", Hour = "09:00", Description = "Text", Priority = TaskPriority.HIGH };

            Controller().Edit(model);

            _tasks.Verify(r => r.Update(It.Is<TaskEntity>(t => t.TaskID == model.TaskID && t.UserID == _signedIn.UserID)), Times.Once);
        }

        [Fact]
        public void Edit_TaskNotFoundForThisUser_SaysSo()
        {
            _tasks.Setup(r => r.Update(It.IsAny<TaskEntity>())).Returns(false);
            var controller = Controller();

            controller.Edit(new TaskEditModel { TaskID = Guid.NewGuid(), Name = "Task", Date = "2026-10-10", Hour = "09:00", Description = "Text", Priority = TaskPriority.HIGH });

            Assert.Equal("Task not found.", controller.TempData["Message"]);
        }

        [Fact]
        public void Delete_UsesTheSignedInUserAsTheOwner()
        {
            var taskId = Guid.NewGuid();
            _tasks.Setup(r => r.Delete(taskId, _signedIn.UserID)).Returns(true);
            var controller = Controller();

            controller.Delete(taskId);

            _tasks.Verify(r => r.Delete(taskId, _signedIn.UserID), Times.Once);
            Assert.Equal("The task was deleted successfully.", controller.TempData["Message"]);
        }

        [Fact]
        public void Delete_SomeoneElsesTask_SaysNotFound()
        {
            _tasks.Setup(r => r.Delete(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            var controller = Controller();

            controller.Delete(Guid.NewGuid());

            Assert.Equal("Task not found.", controller.TempData["Message"]);
        }

        // A GET link could delete a task from any page that gets the user to open it
        [Fact]
        public void Delete_OnlyAcceptsPostWithAnAntiForgeryToken()
        {
            var delete = typeof(TaskController).GetMethod(nameof(TaskController.Delete))!;

            Assert.NotNull(delete.GetCustomAttribute<HttpPostAttribute>());
            Assert.NotNull(delete.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }
    }
}
