using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Updater.Controllers.Filters;
using Updater.Models;

namespace Updater.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [ValidateLoggedUser]
        public IActionResult Index()
        {
            return View();
        }

        [ValidateLoggedUser]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [ValidateLoggedUser]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}