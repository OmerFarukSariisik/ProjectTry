using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Services;

namespace Project.Controllers;

public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly string _connectionString;
    
    public SettingsController(IConfiguration configuration, ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public IActionResult SettingsIndex()
    {
        _settingsService.Initialize();
        return View(_settingsService.AllSettings.First());
    }

    [HttpPost]
    public IActionResult EditSettings(SettingsModel settingsModel)
    {
        _settingsService.EditSettings(settingsModel);
        return RedirectToAction("Index", "Home");
    }
}