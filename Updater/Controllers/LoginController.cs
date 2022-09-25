using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Updater.Helper;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    public class LoginController : Controller
    {
        private IUserRepository _UserRepository;
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
            catch
            {
                return View();
            }
        }

        private ClaimsPrincipal TesteUserCredential(string user, string pass)
        {
            string hashedPass = _UserRepository.HashPass(pass);
            var dbUser = _UserRepository.GetAll(x => x.Username.ToLower().Equals(user) && x.Password.Equals(hashedPass)).FirstOrDefault();

            if(user != null)
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
