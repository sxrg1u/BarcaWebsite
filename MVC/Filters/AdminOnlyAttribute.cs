using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MVC.Data;
using MVC.Models;

namespace MVC.Filters
{
    // Laesst nur eingeloggte Admins durch. Die Rolle wird bei jeder Anfrage in der DB geprueft,
    // damit ein entzogenes Admin-Recht sofort gilt und nicht erst nach dem naechsten Login.
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int? userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
                return;
            }

            var users = context.HttpContext.RequestServices.GetRequiredService<UserRepository>();
            if (users.GetRole(userId.Value) != User.RoleAdmin)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
