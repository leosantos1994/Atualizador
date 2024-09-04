using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Updater.Controllers.Filters;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;
using X.PagedList;

namespace Updater.Controllers
{
    public class UserController : Controller
    {
        private IUserClientRepository _UserRepository;
        public UserController(AppDBContext context)
        {
            this._UserRepository = new UserRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index(int page = 1)
        {
            var users = GetViewModel().OrderByDescending(x => x.Creation).ToPagedList(page, 10);
            foreach (var x in users)
            {
                x.Role = (x.Role == Constants.Role_Client ? "Cliente" : "Administrador");
            }
            ViewData["Users"] = users;
            return View();
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public JsonResult AllClientUsers()
        {
            return Json(GetViewModel(x => x.Role == Constants.Role_Client && x.Clients == null).ToList().Select(x => new { text = x.Username, id = x.Id }));
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditUser(Guid id)
        {
            ModelState.Clear();

            if (id != Guid.Empty)
            {
                return base.PartialView("_EditUserPartial", new Models.User().GetUserViewModel(_UserRepository.Get(id)));
            }
            else
            {
                return PartialView("_EditUserPartial", new UserViewModel());
            }
        }

        [HttpPost]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditUser(UserViewModel viewModel)
        {
            ModelState.Clear();

            try
            {
                if (ValidateModel(viewModel))
                {
                    var userModel = viewModel.GetUserModel(viewModel);
                    if (_UserRepository.Any(x => x.Username.ToLower().Equals(userModel.Username) && x.Id != userModel.Id))
                        throw new Exception("Já existe um usuário com este nome.");

                    VerifyPasswordChanged(userModel);

                    if (viewModel.Id == Guid.Empty)
                    {
                        _UserRepository.Insert(userModel);
                        ModelState.AddModelError("", "Usuário cadastrado com sucesso.");
                    }
                    else
                    {
                        _UserRepository.Update(userModel);

                        ModelState.AddModelError("", "Usuário alterado.");
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
            }
            return PartialView("_EditUserPartial", viewModel);
        }

        private bool ValidateModel(UserViewModel viewModel)
        {
            switch ((Enums.UserRole)int.Parse(viewModel.Role))
            {
                case Enums.UserRole.client:
                    viewModel.Role = Constants.Role_Client;
                    break;
                case Enums.UserRole.sysadm:
                    viewModel.Role = Constants.Role_ADM;
                    break;
                default:
                    viewModel.Role = Constants.Role_Client;
                    break;
            }
            viewModel.Locked = HttpContext.Request.Form["Locked"].ToString().ToLower() == "on" ? true : false;

            if (string.IsNullOrEmpty(viewModel.Password))
                throw new Exception("Senha não foi informada");

            if (string.IsNullOrEmpty(viewModel.Username))
                throw new Exception("Usuário não foi informado");

            return true;
        }

        private IEnumerable<UserViewModel> GetViewModel()
        {
            foreach (var item in _UserRepository.GetAll().ToList())
            {
                yield return item;
            }
        }

        private IEnumerable<UserViewModel> GetViewModel(Expression<Func<Models.User, bool>> predicate)
        {
            foreach (var item in _UserRepository.GetAll(predicate).ToList())
            {
                yield return item;
            }
        }

        private void VerifyPasswordChanged(User user)
        {
            User dbUser = _UserRepository.GetAll(x => x.Id == user.Id).FirstOrDefault();
            if (dbUser != null)
            {
                if (dbUser.Password != user.Password)
                    user.Password = _UserRepository.HashPass(user.Password);
                return;
            }
            user.Password = _UserRepository.HashPass(user.Password);
        }
    }
}
