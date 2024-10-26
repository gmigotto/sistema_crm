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
        private readonly AsaasClient _asaasClient;

        public HomeController(AsaasClient asaasClient)
        {
            _asaasClient = asaasClient;
        }
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
        public async Task<IActionResult> FinalizarCompra(PaymentModel pagamento)
        {
            string cpfSanitizado = pagamento.Cpf.Replace("-", "").Replace(".", "");

            var customerData = new Dictionary<string, object>
            {
                { "name", pagamento.NomeTitular },
                { "cpf", cpfSanitizado },
                { "phone", pagamento.Telefone }
            };

            AssasModel asaasModel = new AssasModel();
            var verificacao = asaasModel.VerificaSeExisteAsaasCustomer(cpfSanitizado);
            string? asaas_customer = "";
            if (verificacao == null)
            {
                var response = await _asaasClient.CreateCustomerAsync(customerData);

                if (response != null && response.ContainsKey("id"))
                {
                    asaasModel.Cpf = cpfSanitizado;
                    asaasModel.Asaas = response["id"].ToString();
                    asaas_customer = response["id"].ToString();
                    asaasModel.GravarCustomerAsaas();
                }
                else
                {
                    return BadRequest("Falha ao criar cliente no Asaas.");
                }
            }
            else
            {
                asaas_customer = verificacao.Asaas;
            }

            var paymentData = new Dictionary<string, object>
            {
                { "card_info", new Dictionary<string, object>
                    {
                        { "name", pagamento.NomeTitular },
                        { "number", pagamento.NumeroCartaoDeCredito },
                        { "expiry_month", pagamento.MesExpiracao },
                        { "expiry_year", pagamento.AnoExpiracao },
                        { "cvv", pagamento.cvv }
                    }
                },
                { "card_holder_info", new Dictionary<string, object>
                    {
                        { "name", pagamento.NomeTitular },
                        { "cpf", cpfSanitizado },
                        { "email", pagamento.Email },
                        { "cep", pagamento.Cep },
                        { "number", pagamento.NumeroCasa },
                        { "phone", pagamento.Telefone }
                    }
                },
                { "customer", asaas_customer },
                { "total_value", Convert.ToDecimal(pagamento.valor) }
            };

            var response2 = await _asaasClient.CreateOrderAsync(paymentData);

            return RedirectToAction("PrimeiroAcesso", "Home");
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