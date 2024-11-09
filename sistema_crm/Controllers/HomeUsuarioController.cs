using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using sistema_crm.Uteis;
using System.Data;

namespace sistema_crm.Controllers
{
    public class HomeUsuarioController : Controller
    {
        
        public IActionResult Menu(HomeUsuarioModel home)
        {
            string userId = HttpContext.Session.GetString("IdUsuarioLogado");

            int id = int.Parse(userId);
            var model = home.RetornarNegociacao(id);
            
            ViewBag.Negociacao = model.Negociacao;
             return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
