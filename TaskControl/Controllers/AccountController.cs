using TaskControl.Data.Entities;
using TaskControl.Data.Interfaces;
using TaskControl.Messeges;
using TaskControl.Models;
using TaskControl.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TaskControl.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly string _appUrl;

        public AccountController(IUserRepository userRepository, IPasswordResetRepository passwordResetRepository,
            IEmailService emailService, ILogger<AccountController> logger, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _emailService = emailService;
            _logger = logger;
            _appUrl = (configuration["AppUrl"] ?? throw new InvalidOperationException("Set AppUrl, the address used in password reset links.")).TrimEnd('/');
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
                // The same answer whether or not the e-mail has an account, so this page
                // cannot be used to find out who is registered
                TempData["Message"] = "If this email has an account, we sent it a link to choose a new password. The link is valid for 30 minutes.";
                ModelState.Clear();
                try
                {
                    var user = _userRepository.Get(model.Email);
                    if (user != null)
                    {
                        SendPasswordResetLink(user);
                    }
                }
                catch (Exception e)
                {
                    // Showing the error would reveal that the e-mail has an account
                    _logger.LogError(e, "The password reset link could not be sent");
                }
            }
            return View();
        }

        // The password is not changed here: the e-mail carries a single-use link and the
        // person chooses the new password on ResetPassword
        private void SendPasswordResetLink(User user)
        {
            var token = PasswordResetToken.NewToken();
            _passwordResetRepository.Create(user.UserID, PasswordResetToken.Hash(token), DateTime.UtcNow.Add(PasswordResetToken.Lifetime));

            // Built from AppUrl, not from the request's Host header, which a client can forge
            // to make the e-mailed link point to their own site
            var link = $"{_appUrl}/Account/ResetPassword?token={Uri.EscapeDataString(token)}";
            var body = $@"
                        <div style='text-align: center; margin:40px; padding: 60px; border: 2px solid #ccc; font-size: 16pt;'>
                         Hello <strong>{WebUtility.HtmlEncode(user.Name)}</strong>,
                         <br/><br/>
                         Use this link to choose a new password. It works once and is valid for 30 minutes:
                         <br/><br/>
                         <a href='{link}'>Choose a new password</a>
                         <br/><br/>
                         If you did not ask for it, ignore this e-mail. Your password has not changed.
                        </div>
                        ";
            _emailService.SendEmail(user.Email, "Choose a new password - Task Control System", body);
        }

        public IActionResult ResetPassword(string token)
        {
            return View(new AccountResetPasswordModel { Token = token });
        }

        [HttpPost]
        public IActionResult ResetPassword(AccountResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _passwordResetRepository.Consume(PasswordResetToken.Hash(model.Token), DateTime.UtcNow);
            if (userId == null)
            {
                TempData["Message"] = "This link is invalid, has expired or was already used. Ask for a new one.";
                return RedirectToAction("Passwordrecovery");
            }

            _userRepository.Update(userId.Value, model.NewPassWord);
            TempData["Message"] = "Your password was changed. Log in with the new one.";
            return RedirectToAction("Login");
        }
    }
}
