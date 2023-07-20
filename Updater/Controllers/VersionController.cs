using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MidModel;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;
using X.PagedList;

namespace Updater.Controllers
{
    public class VersionController : Controller
    {
        private IVersionFileRepository _FileRepository;
        private IVersionRepository _VersionRepository;
        private IServiceRepository _ServiceRepository;
        public VersionController(AppDBContext context)
        {
            this._VersionRepository = new VersionRepository(context);
            this._FileRepository = new VersionFileRepository(context);
            this._ServiceRepository = new ServiceRepository(context);
        }

        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index(string? error, int page = 1)
        {
            var versions = GetViewModel().ToList();
            versions.ForEach(x => x.FileName = _FileRepository.GetFileName(x.Id));
            ViewData["Versions"] = versions.OrderByDescending(x => x.Date).ToPagedList(page, 10);
            ViewData["MSG"] = error;

            return View();
        }

        [HttpGet]
        public ActionResult EditVersion(Guid id)
        {
            ModelState.Clear();

            if (id != Guid.Empty)
            {
                var versionViewModel = new Models.Version().GetVersionViewModel(_VersionRepository.Get(id));
                versionViewModel.VersionFileId = _FileRepository.GetIdByVersion(versionViewModel.Id);

                return base.PartialView("_EditVersionPartial", versionViewModel);
            }
            else
            {
                return base.PartialView("_EditVersionPartial", new VersionViewModel());
            }
        }

        [HttpPost]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        public ActionResult EditVersion(VersionViewModel viewModel)
        {
            try
            {
                ModelState.Clear();

                if (ValidateModel(viewModel))
                {
                    var versionModel = viewModel.GetVersionModel(viewModel);

                    if (viewModel.Id == Guid.Empty)
                    {
                        if (viewModel.File != null && !string.IsNullOrEmpty(viewModel.File.FileName))
                        {
                            _VersionRepository.Insert(versionModel);

                            using (var memoryStream = new MemoryStream())
                            {
                                viewModel.File.CopyTo(memoryStream);
                                VersionFile file = new(memoryStream.ToArray(), versionModel.Id, viewModel.File.FileName);
                                file.VersionId = versionModel.Id;
                                _FileRepository.Insert(file);
                            }

                            viewModel.File = null;

                            ModelState.AddModelError("", "Registro criado.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Arquivo não informado.");
                        }
                    }
                    else
                    {
                        _VersionRepository.Update(versionModel);

                        if (viewModel.File != null && !string.IsNullOrEmpty(viewModel.File.FileName))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                viewModel.File.CopyTo(memoryStream);

                                VersionFile file = _FileRepository.GetByVersion(versionModel.Id) ?? new();

                                bool create = file.VersionId == Guid.Empty;

                                file.File = memoryStream.ToArray();
                                file.FileName = viewModel.File.FileName;
                                file.VersionId = versionModel.Id;

                                if (create)
                                    _FileRepository.Insert(file);
                                else
                                    _FileRepository.Update(file);
                            }
                            viewModel.File = null;
                            ModelState.AddModelError("", "Registro alterado.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Verifique as informações.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro: " + ex.Message);
                viewModel.File = null;
            }
            return PartialView("_EditVersionPartial", viewModel ?? new());
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Delete(Guid itemid)
        {
            ModelState.Clear();

            try
            {
                var version = _VersionRepository.Get(itemid);

                var service = _ServiceRepository.Get(x => x.VersionId == version.Id);

                if (service != null && (service.ScheduleProgress == ScheduleProgress.Done || service.ScheduleProgress == ScheduleProgress.Started))
                    return RedirectToAction("Index", new { error = "Versão possuí um agendamento em execução ou concluído e não pode ser excluída." });

                var file = _FileRepository.GetByVersion(version.Id);

                if (file != null)
                    _FileRepository.Delete(file);

                if (service != null)
                    _ServiceRepository.Delete(service);

                _VersionRepository.Delete(version);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new { error = "Ocorreu um erro não tratado na exclusão. " + ex.Message });
            }

            return RedirectToAction("Index", new { error = "Versão excluída com sucesso. " });
        }

        private bool ValidateModel(VersionViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Patch))
                throw new Exception("Patch é obrigatório.");

            if (string.IsNullOrEmpty(viewModel.Version))
                throw new Exception("Versão é obrigatória.");

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
