using Microsoft.AspNetCore.Mvc;

namespace sistema_crm.Controllers
{
    public class HomeUsuarioController : Controller
    {
        public IActionResult Menu()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
