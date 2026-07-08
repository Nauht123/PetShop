// Controllers/AdminDashboardController.cs
using Microsoft.AspNetCore.Mvc;

namespace PetShop.Controllers
{
    public class AdminDashboardController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("VaiTro") != "Admin")
                return RedirectToAction("Login", "Account");

            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }
    }
}