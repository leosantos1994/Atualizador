using Microsoft.AspNetCore.Mvc;
using Updater.Models;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class UserController : Controller
    {
        private IUserRepository _UserRepository;
        public UserController(AppDBContext context)
        {
            this._UserRepository = new UserRepository(context);
        }

        // GET: VersaoController
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult Index()
        {
            ViewData["Users"] = GetViewModel().ToList();
            return View();
        }

        [HttpGet]
        [ValidateLoggedUser]
        [ValidateLoggedUserAdm]
        public ActionResult EditUser(Guid id)
        {
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
                        ViewData["error"] = "Usuário cadastrado com sucesso.";
                    }
                    else
                    {
                        _UserRepository.Update(userModel);

                        ViewData["error"] = "Usuário alterado";
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
            return PartialView("_EditUserPartial", viewModel);
        }

        private bool ValidateModel(UserViewModel viewModel)
        {
            switch ((Enums.UserRole)int.Parse(viewModel.Role))
            {
                case Enums.UserRole.client:
                    viewModel.Role = "client";
                    break;
                case Enums.UserRole.sysadm:
                    viewModel.Role = "sysadm";
                    break;
                default:
                    viewModel.Role = "client";
                    break;
            }
            viewModel.Locked = HttpContext.Request.Form["Locked"].ToString().ToLower() == "on" ? true : false;
            return true;
        }

        private IEnumerable<UserViewModel> GetViewModel()
        {
            foreach (var item in _UserRepository.GetAll().ToList())
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
