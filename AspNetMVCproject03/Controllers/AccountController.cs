using AspNetMVCproject03.Data.Entities;
using AspNetMVCproject03.Data.Interfaces;
using AspNetMVCproject03.Messeges;
using AspNetMVCproject03.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetMVCproject03.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AccountLoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_userRepository.Get(model.Email, model.PassWord) != null)
                    {
                        //User Auth
                        var auth = new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.Name, model.Email) },
                            CookieAuthenticationDefaults.AuthenticationScheme);

                        //Creating cookie
                        var claim = new ClaimsPrincipal(auth);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claim);

                        //redirect to /Home/Index
                                                //VIEW  CONTROLLER
                        return RedirectToAction("Index", "Home");
                    }
                    else 
                    {
                        TempData["Message"] = $"Invalid user or password";
                    }
                }
                catch (Exception e)
                {

                    TempData["Message"] = "Erro: " + e.Message;
                }
            }
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(AccountRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User();
                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.PassWord = model.PassWord;

                    //email verification
                    if(_userRepository.Get(user.Email) != null)
                    {
                        TempData["Message"] = $"Email {user.Email} already exists";
                    }
                    else
                    {
                        //creating
                        _userRepository.Create(user);
                        TempData["Message"] = $"User {user.Name} registred successfully";
                        ModelState.Clear();
                    }


                }
                catch (Exception e)
                {

                    TempData["Message"] = "Erro: " + e.Message;
                }

                
            }
            return View();
        }
        public IActionResult Logout()
        {
            //Delete user cookie
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Passwordrecovery()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Passwordrecovery(AccountPasswordRecoveryModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _userRepository.Get(model.Email);
                    if (user != null)
                    {
                        var newPassword = new Random().Next(99999999, 999999999).ToString();
                        _userRepository.Update(user.UserID, newPassword);
                        //send email

                        var to = user.Email;
                        var subject = "New password - Task Control System";
                        var body = $@"
                        <div style='text-align: center; margin:40px; padding: 60px; borde: 2px solid #ccc; font-size: 16pt;'>
                         <br/>
                        Hello <strong>{user.Name}</strong>,
                         <br/>
                         System generats a new password for you access your account.
                         <br/>
                         Please use this password: <strong>{newPassword}</strong>
                         <br/>
                         After to login you could to change this password for one that you prefer.
                         <br/>
                         Best Regards.
                        </div>
                        ";

                        var message = new EmailServiceMessage();
                        message.SendEmail(to, subject, body);
                        TempData["Message"] = $"New password are generated successfully and sent to your email '{user.Email}' .";
                        ModelState.Clear();
                    }
                    else
                    {
                        TempData["Message"] = "The email doesn't exists in this system.";
                    }
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
