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

        public ActionResult GraficoVendas3meses()
        {

            List<RelatorioModel> lista = new RelatorioModel().RetornarGraficoVendasUltimosMeses();
            ViewBag.RetornarGraficoVendasUltimosMeses = lista;

            string valores = "";
            string labels = "";
            string cores = "";

            var random = new Random();
            // Percorre a lista de itens para compor o gráfico de barras
            for (int i = 0; i < lista.Count; i++)
            {
                valores += lista[i].Valor.ToString() + ",";
                labels += "'" + lista[i].Mes.ToString() + "',";
                // escolher aleatoriamente as cores para compor as barras
                cores += "'" + String.Format("#{0:X6}", random.Next(0x1000000)) + "',";
            }

            ViewBag.Valores = valores.TrimEnd(',');
            ViewBag.Labels = labels.TrimEnd(',');
            ViewBag.Cores = cores.TrimEnd(',');


            return View();
        }
        public ActionResult GraficoStatus()
        {
            List<RelatorioModel> lista = new RelatorioModel().RetornarGraficoValoresStatus();
            ViewBag.RetornarGraficoValoresStatus = lista;

            string valores = "";
            string labels = "";
            string cores = "";

            // Percorre a lista de itens para compor o gráfico de pizza com cores específicas
            for (int i = 0; i < lista.Count; i++)
            {
                valores += lista[i].Valor.ToString() + ",";
                labels += "'" + lista[i].Status.ToString() + "',";

                // Define a cor com base no status
                if (lista[i].Status == "VENDA REALIZADA")
                {
                    cores += "'#66bb6a',";
                }
                else if (lista[i].Status == "PERDIDO")
                {
                    cores += "'red',";
                }
                else if (lista[i].Status == "CANCELADO")
                {
                    cores += "'gray',";
                }
                else
                {
                    // Cor padrão caso o status não esteja entre as opções definidas
                    cores += "'#CCCCCC',";
                }
            }

            ViewBag.Valores1 = valores.TrimEnd(',');
            ViewBag.Labels1 = labels.TrimEnd(',');
            ViewBag.Cores1 = cores.TrimEnd(',');


            return View();
        }

        public IActionResult Menu(VendedorModel vendedor, HomeModel home, RelatorioModel relatorio)
        {
            var ranking = vendedor.ObterTop5Vendedores(); // Chama a função para obter o ranking dos vendedores.
            ViewBag.RankingVendas = ranking;

            var model = home.RetornarNegociacao(); // Chama a função que retorna os dados de negociação.
            ViewBag.Negociacao = model.Negociacao;

            var taxa = home.TaxaConversaoPropostas();
            ViewBag.Conversao = taxa;

            ViewBag.ActivePage = "Home";

            GraficoVendas3meses();
            GraficoStatus();
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
            return View();
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
        public async Task<IActionResult> FinalizaCompra(PaymentModel pagamento)
        {
            string cpfSanitizado = pagamento.Cpf.Replace("-", "").Replace(".", "");

            // Validação simples de cartão
            if (!ValidarCartao(pagamento.NumeroCartaoDeCredito, pagamento.MesExpiracao, pagamento.AnoExpiracao, pagamento.cvv))
            {
                TempData["ErroPagamento"] = "Houve um problema com o pagamento. Verifique os dados do cartao.";
                return RedirectToAction("Erro");
            }

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
            TempData["SuccessMessage"] = "Pagamento aprovado!";


            return RedirectToAction("SuccessPage", "Home");
        }

        // Função para validar o cartão de crédito
        private bool ValidarCartao(string numeroCartao, string mesExpiracao, string anoExpiracao, string cvv)
        {
            // Validação do número do cartão
            if (string.IsNullOrWhiteSpace(numeroCartao) || numeroCartao.Length < 13 || numeroCartao.Length > 19)
                return false;

            // Validação do mês de expiração
            if (!int.TryParse(mesExpiracao, out int mes) || mes < 1 || mes > 12)
                return false;

            // Validação do ano de expiração
            if (!int.TryParse(anoExpiracao, out int ano) || ano < DateTime.Now.Year ||
                (ano == DateTime.Now.Year && mes < DateTime.Now.Month))
                return false;

            // Validação do CVV
            if (string.IsNullOrWhiteSpace(cvv) || cvv.Length < 3 || cvv.Length > 4)
                return false;

            return true;
        }

        [HttpPost]

        public IActionResult FinalizarCompra(HomeModel home)
        {
            TempData["SuccessMessage"] = "Pagamento aprovado!";
            return RedirectToAction("SucessPage", "Home");
        }



        [HttpGet]
        public IActionResult TelaCompra()
        {

            return View();

        }

        [HttpGet]
        public IActionResult SuccessPage()
        {


            return View();

        }

        [HttpPost]
        public IActionResult SuccessPage(LoginModel login)
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

        [HttpGet]
        public IActionResult Erro()
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
                HttpContext.Session.SetString("IdAdminLogado", login.Id);
                HttpContext.Session.SetString("NomeAdminLogado", login.Nome);
                return RedirectToAction("Menu", "Home");
            }
            else
            {
                TempData["ErrorLogin"] = "E-mail ou Senha são inválidos!";
            }

            return View();
            
        }

        public IActionResult RankingVendas(VendedorModel vendedor)
        {
            var dal = new DAL();
            var ranking = vendedor.ObterTop5Vendedores(); // Supondo que este método esteja disponível no controller.
            ViewBag.RankingVendas = ranking;
            return View(ranking);
        }
    }
}