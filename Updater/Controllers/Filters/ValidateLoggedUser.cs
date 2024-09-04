using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Updater.Controllers.Filters
{
    public class ValidateLoggedUser : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (!context.HttpContext.User.Identity.IsAuthenticated ||
                    !context.HttpContext.User.Claims.Any(x => x.Type.ToLower().EndsWith("expiration") && DateTime.Parse(x.Value) > DateTime.Now))
                    context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Login" },
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
