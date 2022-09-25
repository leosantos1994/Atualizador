using Microsoft.AspNetCore.Mvc;

namespace Updater.Controllers
{
    public class UnauthorizedController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
