using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;
using Updater.Helper;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class LoginController : Controller
    {
        private IUserClientRepository _UserRepository;
        public LoginController(AppDBContext context)
        {
            this._UserRepository = new UserRepository(context);
        }

        public ActionResult Index(string error)
        {
            ViewData.TryAdd("error", error ?? "");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IFormCollection collection)
        {
            try
            {
                //_UserRepository.Insert(new Models.User() { Username = "adm", Password = _UserRepository.HashPass("adm"), Role = "sysadm" });
                //var credential = TesteUserCredential("adm", "adm");
                var credential = TesteUserCredential(collection["user"].ToString(), collection["pass"].ToString());
                if (credential is not null)
                {
                    HttpContext.SignInAsync(credential);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Index("Usuário ou senha incorretos");
                }
            }
            catch(Exception ex)
            {
                return View("Erro ao fazer login: "+ ex.Message);
            }
        }

        private ClaimsPrincipal TesteUserCredential(string user, string pass)
        {
            string hashedPass = _UserRepository.HashPass(pass);
            var dbUser = _UserRepository.GetAll(x => x.Username.ToLower().Equals(user) && x.Password.Equals(hashedPass)).FirstOrDefault();

            if (dbUser != null)
            {
                ClaimsPrincipal claims = TokenService.GenerateClaim(new() { Role = dbUser.Role, Username = dbUser.Username });
                return claims;
            }
            return null;
        }

        public ActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
