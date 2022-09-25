using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Updater.Controllers
{
    public class ValidateLoggedUserAdm : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if(!context.HttpContext.User.Claims.Any(x => x.Type.ToLower().EndsWith("role") && x.Value.ToLower().Equals("sysadm")))
                    context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Unauthorized" },
                        { "action", "Index" }
                    });
            }
            catch
            {
                context.Result = new RedirectToRouteResult(
                      new RouteValueDictionary
                      {
                        { "controller", "Login" },
                        { "action", "Index" }
                      });
            }
        }
    }
}
