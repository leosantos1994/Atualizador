using Microsoft.AspNetCore.Mvc;
using MidModel;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;
using X.PagedList;

namespace Updater.Controllers
{
    public class UpdateSetupController : Controller
    {
        private IServiceRepository _ServiceRepository;
        private IVersionFileRepository _VersionFileRepository;
        private IVersionRepository _VersionRepository;
        private IClientRepository _ClientRepository;
        private IClientUserRepository _ClientUserRepository;
        public UpdateSetupController(AppDBContext context)
        {
            this._VersionRepository = new VersionRepository(context);
            this._ServiceRepository = new ServiceRepository(context);
            this._ClientRepository = new ClientRepository(context);
            this._ClientUserRepository = new ClientUserRepository(context);
            this._VersionFileRepository = new VersionFileRepository(context);
        }

        [HttpGet]
        [ValidateLoggedUser]
        public ActionResult Index(int page = 1)
        {
            var services = IsAdm() ?
                _ServiceRepository.GetAll().OrderByDescending(x => x.ScheduledDate).ToPagedList(page, 10) :
                _ServiceRepository.GetAll(x => x.ClientId == GetClientByUser().Id).OrderByDescending(x => x.ScheduledDate).ToPagedList(page, 10);

            ViewData.Add("Updates", services);
            return View();
        }

        [HttpGet]
        [ValidateLoggedUser]
        public ActionResult EditUpdate(Guid id)
        {
            if (id != Guid.Empty)
            {
                return base.PartialView("_EditUpdateSetupPartial", GetModel(id));
            }
            else
            {
                var model = new UpdateSetupViewModel();
                model.Versions = _VersionRepository.GetAll(x => x.Locked == false).ToList();

                if (IsAdm())
                    model.Clients = _ClientRepository.GetAll(x => x.Locked == false).ToList();
                else
                    model.Clients = new List<Client> { GetClientByUser() };

                return PartialView("_EditUpdateSetupPartial", model);
            }
        }

        private bool IsAdm()
        {
            return HttpContext.User.Claims.Any(x => x.Type.ToLower().EndsWith("role") && x.Value.ToLower().Equals("sysadm"));
        }

        private Client GetClientByUser()
        {
            string name = HttpContext.User.Claims.FirstOrDefault(x => x.Type.ToLower().EndsWith("name")).Value.ToLower();
            return _ClientUserRepository.GetAll(x => x.User.Username.ToLower() == name).FirstOrDefault().Client;
        }

        [HttpPost]
        [ValidateLoggedUser]
        public ActionResult EditUpdate(UpdateSetupViewModel viewModel)
        {
            try
            {
                var serviceModel = GetServiceModel(viewModel);

                if (viewModel.Id == Guid.Empty)
                {
                    _ServiceRepository.Insert(serviceModel);
                }
                else
                {
                    _ServiceRepository.Update(serviceModel);
                }

                ViewData["error"] = "Atualização agendada com sucesso.";
            }
            catch (Exception ex)
            {
                ViewData["error"] = "Erro: " + ex.Message;
            }
            return PartialView("_EditUpdateSetupPartial", viewModel);
        }

        [HttpPost]
        [ValidateLoggedUser]
        public string Delete(Guid id)
        {
            try
            {
                var service = _ServiceRepository.Get(id);
                if (service.ScheduleProgress == ScheduleProgress.Waiting
                    || service.ScheduleProgress == ScheduleProgress.Error)
                {
                    _ServiceRepository.Delete(service);
                    return "Registro excluído com sucesso";
                }

                return "Registro está em execução ou já executado e não podera ser excluído";
            }
            catch (Exception ex)
            {
                return "Ocorreu um erro ao tentar excluir " + ex.Message;
            }
        }

        private ServiceModel GetServiceModel(UpdateSetupViewModel viewModel)
        {
            ServiceModel model = new();
            var client = _ClientRepository.GetAll(x => x.Id == Guid.Parse(Request.Form["client"][0].ToString())).FirstOrDefault();
            var versionFileID = _VersionFileRepository.GetIdByVersion(Guid.Parse(Request.Form["version"][0].ToString()));
            var version = _VersionRepository.Get(Guid.Parse(Request.Form["version"][0].ToString()));

            model.IsPool = Request.Form["IsPool"].Any() ? true : false;
            model.IsService = Request.Form["IsService"].Any() ? true : false;
            model.SiteUser = client.SiteUser;
            model.SitePass = client.SitePass;
            model.HasUpdate = true;
            model.ScheduledDate = viewModel.ScheduledDate;
            model.ClientId = client.Id;
            model.ClientName = client.Name;
            model.VersionName = version.ProductVersion + " - " + version.Patch;
            model.VersionFileId = versionFileID;
            model.VersionId = version.Id;
            model.PoolName = client.AppPoolName;
            model.ServiceName = client.ServiceName;
            return model;
        }

        private UpdateSetupViewModel GetModel(Guid Id)
        {
            var model = _ServiceRepository.GetAll(x => x.Id == Id).FirstOrDefault();

            return new()
            {
                IsService = model.IsService,
                ScheduledDate = model.ScheduledDate,
                IsPool = model.IsPool,
                Id = Id,
                VersionId = model.VersionId,
                Clients = new() { _ClientRepository.Get(model.ClientId) },
                Versions = new() { _VersionRepository.Get(model.VersionId) },
            };
        }
    }
}