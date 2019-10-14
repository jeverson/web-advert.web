using Amazon.Extensions.CognitoAuthentication;
using Amazon.AspNetCore.Identity.Cognito;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;
using System;
using Amazon.CognitoIdentityProvider;

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

        public IActionResult Signup() => View();

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


        public IActionResult Confirm() => View();

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                AddUserNotFoundModelState();
                return View(model);
            }

            try
            {
                await user.ConfirmSignUpAsync(model.Code, true);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("SignUpError", $"Failed to Confirm SignUp. Error: {e.Message}");
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

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                AddUserNotFoundModelState();
                return View(model);
            }

            var authResponse = await user.StartWithSrpAuthAsync(new InitiateSrpAuthRequest { Password = model.Password });

            if (authResponse.AuthenticationResult != null)
                return RedirectToAction("Index", "Home");
            else if (authResponse.ChallengeName == ChallengeNameType.SMS_MFA)
                return RedirectToAction("TwoFactorAuth", new TwoFactorAuthModel { Email = model.Email });
            else
                ModelState.AddModelError("LoginError", "Email or password is not valid.");

            return View(model);
        }

        private void AddUserNotFoundModelState() => ModelState.AddModelError("NotFound", "A user with the given email address was not found.");

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                AddUserNotFoundModelState();
                return View(model);
            };

            await user.ForgotPasswordAsync();

            return RedirectToAction("ConfirmForgotPassword");

        }

        public IActionResult ConfirmForgotPassword() => View();

        [HttpPost]
        public async  Task<IActionResult> ConfirmForgotPassword(ConfirmForgotPasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                AddUserNotFoundModelState();
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

            var loginModel = new LoginModel { Email = model.Email };
            return RedirectToAction("Login", loginModel);
        }

        public IActionResult TwoFactorAuth() => View();

        public async Task<IActionResult> TwoFactorAuth(TwoFactorAuthModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                AddUserNotFoundModelState();
                return View(model);
            }

            try
            {
                await user.RespondToSmsMfaAuthAsync(
                    new RespondToSmsMfaRequest {
                        SessionID = user.SessionTokens.IdToken,
                        MfaCode = model.Code
                    });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("MFAFailed", $"Failed to validate the provided code. Err: {e.Message}");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
