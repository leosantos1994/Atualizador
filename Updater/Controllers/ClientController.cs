using Microsoft.AspNetCore.Mvc;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class ClientController : Controller
    {
        private IClientRepository _ClientRepository;
        private IUserRepository _UserRepository;
        public ClientController(AppDBContext context)
        {
            this._ClientRepository = new ClientRepository(context);
            this._UserRepository = new UserRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index()
        {
            ViewData["Clients"] = GetViewModel().ToList();
            return View();
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditClient(Guid id)
        {
            if (id != Guid.Empty)
            {
                var client = new Models.Client().GetClientViewModel(_ClientRepository.Get(id));
                return base.PartialView("_EditClientPartial", client);
            }
            else
            {
                return PartialView("_EditClientPartial", new ClientViewModel());
            }
        }

        [HttpPost]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditClient(ClientViewModel viewModel)
        {
            try
            {
                if (ValidateModel(viewModel))
                {
                    var clientModel = viewModel.GetClientModel(viewModel);
                    if (_ClientRepository.Any(x => (x.Name.Equals(clientModel.Name) || x.Server.Equals(clientModel.Server)) && x.Id != clientModel.Id))
                    {
                        throw new Exception("Já existe um cliente com este nome ou servidor.");
                    }

                    if (viewModel.Id == Guid.Empty)
                    {
                        _ClientRepository.Insert(clientModel);
                        ViewData["error"] = "Cliente cadastrado com sucesso.";
                    }
                    else
                    {
                        _ClientRepository.Update(clientModel);

                        ViewData["error"] = "Cliente alterado";
                    }
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
            return PartialView("_EditClientPartial", viewModel);
        }

        private bool ValidateModel(ClientViewModel viewModel)
        {
            viewModel.Locked = HttpContext.Request.Form["Locked"].ToString().ToLower() == "on" ? true : false;
            return true;
        }

        private IEnumerable<ClientViewModel> GetViewModel()
        {
            foreach (var item in _ClientRepository.GetAll().ToList())
            {
                yield return item;
            }
        }
    }
}
