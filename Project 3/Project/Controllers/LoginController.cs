using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public IActionResult LoginIndex()
        {
            _loginService.Initialize();
            var loginModel = new LoginModel();
            loginModel.Username = "";
            loginModel.Password = "";
            return View(loginModel);
        }

        [HttpPost]
        public IActionResult LoginIndex(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("LoginIndex", "Login");
            }

            _loginService.Initialize();
            // Authenticate the user using the login service
            bool isAuthenticated = _loginService.AllLoginModels.Exists(
                x => x.Username == model.Username && x.Password == model.Password);

            if (isAuthenticated)
            {
                // Redirect to the dashboard or home page upon successful login
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // If authentication fails, add a model error and return the view
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return RedirectToAction("LoginIndex", "Login");
            }
        }
    }
}