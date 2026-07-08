// Filters/NavCategoryFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PetShop.Data;

namespace PetShop.Filters
{
    public class NavCategoryFilter : IActionFilter
    {
        private readonly AppDbContext _db;

        public NavCategoryFilter(AppDbContext db)
        {
            _db = db;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.NavCategories = _db.Categories.ToList();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}