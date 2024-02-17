using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

public class JobController : Controller
{

    public IActionResult JobIndex()
    {
        return View();
    }
}