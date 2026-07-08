using Microsoft.AspNetCore.Mvc;

namespace PetShop.Controllers
{
    public class PolicyController : Controller
    {
        // GET: /Policy/Shipping
        public IActionResult Shipping()
        {
            return View();
        }

        // GET: /Policy/Genuine
        public IActionResult Genuine()
        {
            return View();
        }

        // GET: /Policy/Return
        public IActionResult Return()
        {
            return View();
        }

        // GET: /Policy/Support
        public IActionResult Support()
        {
            return View();
        }
    }
}