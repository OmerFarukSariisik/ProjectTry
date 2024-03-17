using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

public class TestController : Controller
{
    public IActionResult TestPage()
    {
        return View();
    }
}