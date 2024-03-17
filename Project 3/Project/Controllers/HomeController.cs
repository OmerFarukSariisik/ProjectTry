using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Project.Models;
using Project.Services;

namespace Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITaxManagementService _taxManagementService;

        public HomeController(ITaxManagementService taxManagementService)
        {
            _taxManagementService = taxManagementService;
        }

        public IActionResult Index()
        {
            DateTime today = DateTime.Today;
            DateTime endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            var notification = "No notification";
            var hasNotification = false;
            if ((endOfMonth - today).Days <= 30)
            {
                var totalTaxAmountThisMonth = _taxManagementService.GetThisMonthTaxAmount();
                notification = $"Incoming tax payment amount: {totalTaxAmountThisMonth} AED";
                hasNotification = true;
            }
            
            var homeModel = new HomeModel
            {
                HasNotification = hasNotification,
                NotificationMessage = notification
            };
            return View(homeModel);
        }
    }
}