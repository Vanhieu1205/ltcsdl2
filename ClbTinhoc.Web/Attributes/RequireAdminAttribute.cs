using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClbTinhoc.Web.Models;

namespace ClbTinhoc.Web.Attributes
{
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != UserRoles.Admin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}