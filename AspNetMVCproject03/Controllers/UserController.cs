using AspNetMVCproject03.Data.Interfaces;
using AspNetMVCproject03.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Information()
        {
            try
            {
                var email = User.Identity.Name;
                var user = _userRepository.Get(email);
                TempData["UserID"] = user.UserID;
                TempData["Name"] = user.Name;
                TempData["Email"] = user.Email;
                TempData["RegistrationDate"] = user.RegistrationDate.ToString("dd/MM/yyyy");
            }
            catch (Exception e)
            {

                TempData["Messege"] = e.Message;
            }
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangePassword(UserChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {            
                try
                {

                    var email = User.Identity.Name;
                    var user = _userRepository.Get(email);
                    //Validate if email and password is the same in database
                    if (_userRepository.Get(email, model.ActualPassWord) != null)
                    {
                        //try to update the password only.
                        _userRepository.Update(user.UserID, model.NewPassWord);
                        TempData["Messege"] = "New password updated successfully, use the new password next time you login.";
                    }
                    else
                    {
                        TempData["Messege"] = "Actual password is wrong, try again";
                    }

                }
                catch (Exception e)
                {

                    TempData["Messege"] = e.Message;
                }
            }
            return View();
        }
    }
}
