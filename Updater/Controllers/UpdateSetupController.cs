using Microsoft.AspNetCore.Mvc;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class UpdateSetupController : Controller
    {
        private IVersionFileRepository _FileRepository;
        private IVersionRepository _VersionRepository;
        public UpdateSetupController(AppDBContext context)
        {
            this._VersionRepository = new VersionRepository(context);
            this._FileRepository = new VersionFileRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index()
        {
            //ViewData["Versions"] = GetViewModel().ToList();
            return View();
        }

        [HttpGet]
        public ActionResult EditVersion(Guid id)
        {
            //if (id != Guid.Empty)
            //{
            //    return PartialView("_EditVersionPartial", new VersionModel().GetVersionViewModel(_VersionRepository.Get(id)));
            //}
            //else
            //{
            //    return PartialView("_EditVersionPartial", new VersionViewModel());
            //}
            return View("Index");
        }

    }
}
