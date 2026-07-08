// Filters/AdminAuthFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetShop.Filters
{
    public class AdminAuthFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var vaiTro = session.GetString("VaiTro");

            if (vaiTro != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}