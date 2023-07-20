using Microsoft.AspNetCore.Mvc;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;
using X.PagedList;

namespace Updater.Controllers
{
    public class ClientController : Controller
    {
        private IClientRepository _ClientRepository;
        private IClientUserRepository _ClientUserRepository;
        private IUserClientRepository _UserRepository;
        private IServiceRepository _serviceRepository;
        public ClientController(AppDBContext context)
        {
            this._ClientRepository = new ClientRepository(context);
            this._UserRepository = new UserRepository(context);
            this._ClientUserRepository = new ClientUserRepository(context);
            this._serviceRepository = new ServiceRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index(string? error, int page = 1)
        {
            ViewData["Clients"] = GetViewModel().ToList().OrderByDescending(x=> x.Creation).ToPagedList(page, 10);
            ViewData["MSG"] = error;
            return View();
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditClient(Guid id)
        {
            ModelState.Clear();

            if (id != Guid.Empty)
            {
                var client = new Models.Client().GetClientViewModel(_ClientRepository.Get(id));

                client.Users = _ClientUserRepository.GetAll(x => x.ClientId == client.Id).Select(x => x.User).ToList();

                return base.PartialView("_EditClientPartial", client);
            }
            else
            {
                var view = new ClientViewModel()
                {
                    Users = _UserRepository.GetAll(user => !_ClientUserRepository.GetAll().Select(x => x.UserId).Contains(user.Id) && user.Role == Constants.Role_Client).ToList()
                };
                return PartialView("_EditClientPartial", view);
            }
        }

        [HttpPost]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditClient(ClientViewModel viewModel)
        {
            ModelState.Clear();

            var clientModel = viewModel.GetClientModel(viewModel);
            try
            {
                if (ValidateModel(viewModel))
                {
                    if (_ClientRepository.Any(x => (x.Name.Equals(clientModel.Name) && x.Server.Equals(clientModel.Server)) && x.Id != clientModel.Id))
                    {
                        throw new Exception("Já existe um cliente com este nome para o mesmo servidor.");
                    }

                    Guid userId = Guid.Parse(Request.Form["client-users"][0].ToString());

                    if (viewModel.Id == Guid.Empty)
                    {
                        _ClientRepository.Insert(clientModel);

                        UpdateSelectedUser(clientModel.Id, userId);

                        _ClientRepository.SaveChanges();

                        viewModel.Users = new List<User> { _UserRepository.Get(userId) };

                        ModelState.AddModelError("", "Cliente cadastrado com sucesso.");
                    }
                    else
                    {
                        var dbUser = _ClientUserRepository.GetAll(x => x.Client.Id == clientModel.Id).FirstOrDefault();
                        if (dbUser != null)
                        {
                            dbUser.ClientId = Guid.Empty;
                            _ClientUserRepository.Update(dbUser, false);
                        }

                        UpdateSelectedUser(clientModel.Id, userId);

                        _ClientRepository.Update(clientModel);


                        _ClientRepository.SaveChanges();

                        clientModel.Users = new List<ClientUser>() { dbUser };

                        ModelState.AddModelError("", "Cliente alterado.");
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
                return PartialView("_EditClientPartial", new ClientViewModel());
            }
            return PartialView("_EditClientPartial", new Client().GetClientViewModel(_ClientRepository.Get(clientModel.Id)));
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Delete(Guid itemId)
        {
            ModelState.Clear();

            try
            {
                var client = _ClientRepository.Get(itemId);
                if(client is null)
                return RedirectToAction("Index", new { error = "Cliente não localizado." });


                var clientUser = _ClientUserRepository.GetByClient(client.Id);

                var service = _serviceRepository.GetAll(x => x.ClientId == client.Id).FirstOrDefault();

                if (service != null)
                    return RedirectToAction("Index", new { error = "Registro não pode ser excluído, pois possuí uma atualização vinculada." });

                _ClientUserRepository.Delete(clientUser, false);
                _ClientRepository.Delete(client);
                _ClientRepository.SaveChanges();

                return RedirectToAction("Index", new { error = "Cliente excluído com sucesso. " });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", new { error = "Ocorreu um erro não tratado na exclusão. " + ex.Message });
            }
        }

        private void UpdateSelectedUser(Guid clienId, Guid userId)
        {
            if (userId != Guid.Empty)
            {
                _ClientUserRepository.Insert(new() { ClientId = clienId, UserId = userId }, false);
            }
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
