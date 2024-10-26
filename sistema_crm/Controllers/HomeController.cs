using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using sistema_crm.Uteis;
using Microsoft.AspNetCore.Http;

namespace sistema_crm.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Menu()
        {
            ViewBag.ActivePage = "Home";
            return View();
        }

        [HttpGet]
        public IActionResult Login(int? id)
        {
            //Para realizar o logout
            if (id != null)
            {
                if (id == 0)
                {
                    HttpContext.Session.SetString("IdUsuarioLogado", string.Empty);
                    HttpContext.Session.SetString("NomeUsuarioLogado", string.Empty);
                }
            }
            //Fim

            return View();
        }

        [HttpGet]
        public IActionResult LoginVendedor(int? id)
        {


            if (id != null)
            {
                if (id == 0)
                {
                    HttpContext.Session.SetString("IdUsuarioLogado", string.Empty);
                    HttpContext.Session.SetString("NomeUsuarioLogado", string.Empty);
                }
            }
            //Fim

            return View();
        }

        [HttpPost]
        public IActionResult LoginVendedor(LoginModel login)
        {
            bool loginOk = login.ValidarLogin();
            if (loginOk)
            {
                HttpContext.Session.SetString("IdUsuarioLogado", login.Id);
                HttpContext.Session.SetString("NomeUsuarioLogado", login.Nome);
                return RedirectToAction("Menu", "HomeUsuario");
            }
            else
            {
                TempData["ErrorLogin"] = "E-mail ou Senha são inválidos!";
            }
            return View("Menu", "HomeUsuario");
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Start()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult CadastroAdm(HomeModel home)
        {
            home.GravarGestor();

            return RedirectToAction("FinalizarCompra", "Home");
        }

        [HttpPost]
        public IActionResult PrimeiroAcesso(LoginModel login)
        {
            bool loginOk = login.ValidarLoginAdm();
            // Se a validação do Login for Ok irá para a pagina Menu.
            if (loginOk)
            {

                return RedirectToAction("Menu", "Home");

            }
            else
            {
                TempData["ErrorLogin"] = "E-mail ou senha invalidos";
            }

            return View();

        }
        public IActionResult FinalizarCompra()
        {
            return View();
        }

        [HttpPost]

        public IActionResult FinalizarCompra(HomeModel home)
        {
            // home.GravarGestor();

            return RedirectToAction("PrimeiroAcesso", "Home");
        }

        [HttpGet]
        public IActionResult TelaCompra()
        {

            return View();

        }

        [HttpGet]
        public IActionResult PrimeiroAcesso()
        {

            return View();

        }

        public IActionResult Login(LoginModel login)
        {
            bool loginOk = login.ValidarLoginAdm();
            if (loginOk)
            {
                HttpContext.Session.SetString("IdUsuarioLogado", login.Id);
                HttpContext.Session.SetString("NomeUsuarioLogado", login.Nome);
                return RedirectToAction("Menu", "Home");
            }
            else
            {
                TempData["ErrorLogin"] = "E-mail ou Senha são inválidos!";
            }

            return View();
            
        }
    }
}