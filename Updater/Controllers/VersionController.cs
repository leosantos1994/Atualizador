using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class VersionController : Controller
    {
        private IVersionFileRepository _FileRepository;
        private IVersionRepository _VersionRepository;
        public VersionController(AppDBContext context)
        {
            this._VersionRepository = new VersionRepository(context);
            this._FileRepository = new VersionFileRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index()
        {
            ViewData["Versions"] = GetViewModel().ToList();
            return View();
        }

        [HttpGet]
        public ActionResult EditVersion(Guid id)
        {
            if (id != Guid.Empty)
            {
                return base.PartialView("_EditVersionPartial", new Models.Version().GetVersionViewModel(_VersionRepository.Get(id)));
            }
            else
            {
                return PartialView("_EditVersionPartial", new VersionViewModel());
            }
        }

        [HttpPost] 
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditVersion(VersionViewModel viewModel)
        {
            try
            {
                if (ValidateModel(viewModel))
                {
                    var versionModel = viewModel.GetVersionModel(viewModel);

                    if (viewModel.Id == Guid.Empty)
                    {
                        if (viewModel.File != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                viewModel.File.CopyTo(memoryStream);
                                VersionFile file = new(memoryStream.ToArray(), versionModel.Id, viewModel.File.Name);
                                _FileRepository.Insert(file);
                                versionModel.VersionFile = file;
                            }
                            _VersionRepository.Insert(versionModel);
                            ViewData["error"] = "Registro criado.";
                        }
                        else
                        {
                            ViewData["error"] = "Arquivo não informado.";
                        }
                    }
                    else
                    {
                        _VersionRepository.Update(versionModel);

                        if (viewModel.File != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                viewModel.File.CopyTo(memoryStream);
                                VersionFile file = new(memoryStream.ToArray(), versionModel.Id, viewModel.File.Name);
                                file.Id = versionModel.VersionFile.Id;
                                versionModel.VersionFile = file;
                                _FileRepository.Update(file);
                            }
                            ViewData["error"] = "Registro alterado.";
                        }
                    }
                    ViewData["error"] = "Registro alterado";
                }
                else
                {
                    ViewData["error"] = "Verifique as informações.";
                }
            }
            catch (Exception ex)
            {
                ViewData["error"] = "Erro: " + ex.Message;
            }
            return PartialView("_EditVersionPartial", viewModel);
        }

        private bool ValidateModel(VersionViewModel viewModel)
        {
            viewModel.Locked = HttpContext.Request.Form["Locked"].ToString().ToLower() == "on" ? true : false;
            return true;
        }

        private IEnumerable<VersionViewModel> GetViewModel()
        {
            foreach (var item in _VersionRepository.GetAll().ToList())
            {
                yield return item;
            }
        }
    }
}
