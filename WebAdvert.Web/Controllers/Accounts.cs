using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;
using System;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class Accounts : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public Accounts(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        public IActionResult Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _pool.GetUser(model.Email);
            if (user.Status != null)
            {
                ModelState.AddModelError("UserExists", "User with this email already exists.");
                return View(model);
            }

            user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);
            var createdUser = await _userManager.CreateAsync(user, model.Password);
            if (createdUser.Succeeded)
            {
                return RedirectToAction("Confirm");
            }
            return View();
        }


        public IActionResult Confirm()
        {            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("NotFound", "A user with the given email address was not found.");
                return View(model);
            }

            var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                AddErrorsToModelState(result);
                return View(model);
            }
        }

        private void AddErrorsToModelState(IdentityResult result)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("NotFound", "A user with the given email address was not found.");
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RemembeMe, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("LoginError", "Email or password is not valid.");
            }
            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("NotFound", "A user with the given email address was not found.");
                return View(model);
            };

            await user.ForgotPasswordAsync();

            return RedirectToAction("ConfirmForgotPassword");

        }

        public IActionResult ConfirmForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async  Task<IActionResult> ConfirmForgotPassword(ConfirmForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("NotFound", "A user with the given email address was not found.");
                return View(model);
            };

            try
            {
                await user.ConfirmForgotPasswordAsync(model.Code, model.Password);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Failed", $"A failure has ocurred. Message: {e.Message}");
                return View(model);
            }

            return RedirectToAction("Login");
        }
    }
}
