using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using sistema_crm.Models;
using System.Text;
using System.Text.RegularExpressions;
using X.PagedList;

namespace sistema_crm.Controllers
{
    public class ClienteUsuarioController : Controller
    {

        [HttpGet]
        public IActionResult DownloadClientesCsv()
        {
            var clienteModel = new ClienteUsuarioModel();
            var clientes = clienteModel.ListarClientes(HttpContext); // Obtenha a lista de clientes

            // Gera o CSV
            var csv = new StringBuilder();
            csv.AppendLine("Id,Nome,CNPJ,Endereco,UF,Telefone,Situacao");

            foreach (var cliente in clientes)
            {
                csv.AppendLine($"{cliente.Id},{cliente.Nome},{cliente.CNPJ},{cliente.End},{cliente.UF},{cliente.Telefone},{cliente.Situacao}");
            }

            // Retorna o arquivo CSV
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "clientes.csv");
        }

        [HttpGet]
        public IActionResult Lista(int? page, string searchString)
        {
            var clientes = new ClienteUsuarioModel().ListarClientes(HttpContext);

            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.Nome.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            ViewData["CurrentFilter"] = searchString;

            return View(clientes.ToPagedList(pageNumber, pageSize));

        }

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id != null)
            {
                // Carregar o registro de cliente em uma viewbag

                ViewBag.Cliente = new ClienteUsuarioModel().RetornarCliente(id);
            }

            return View();
        }

        [HttpGet]
        public IActionResult Criar()
        {
            //  ViewBag.ActivePage = "Cliente";
            return View();


        }

        [HttpPost]
        public IActionResult Criar(ClienteUsuarioModel cliente)
        {
            ViewBag.VendedorId = HttpContext.Session.GetString("IdUsuarioLogado");

            if (!ValidarCNPJ(cliente.CNPJ))
            {
                ModelState.AddModelError("CNPJ", "CNPJ inválido");
                return View();
            }
            else
            {
                cliente.Gravar(HttpContext);
                return RedirectToAction("Lista", "ClienteUsuario");
            }


        }

        public bool ValidarCNPJ(string CNPJ)
        {
            // Remove caracteres não numéricos do CNPJ
            CNPJ = Regex.Replace(CNPJ, "[^0-9]", "");

            if (CNPJ.Length != 14) // Verifica se o CNPJ tem 14 dígitos
                return false;

            int[] multiplicadoresPrimeiroDigito = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadoresSegundoDigito = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string digitos = CNPJ.Substring(0, 12); // Pega os 12 primeiros dígitos do CNPJ

            int calculo = 0;

            for (int i = 0; i < 12; i++)
            {
                calculo += int.Parse(digitos[i].ToString()) * multiplicadoresPrimeiroDigito[i];
            }

            int resto = calculo % 11;

            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            digitos += primeiroDigito;

            calculo = 0;

            for (int i = 0; i < 13; i++)
            {
                calculo += int.Parse(digitos[i].ToString()) * multiplicadoresSegundoDigito[i];
            }

            resto = calculo % 11;

            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            return CNPJ.EndsWith(primeiroDigito.ToString() + segundoDigito.ToString());
        }



        //Receber os dados do formulario
        [HttpPost]
        public IActionResult Editar(ClienteUsuarioModel cliente)
        {

            ViewBag.VendedorId = HttpContext.Session.GetString("IdUsuarioLogado");
            cliente.Gravar(HttpContext);
            return RedirectToAction("Lista", "ClienteUsuario");




        }


        public IActionResult Excluir(int id)
        {
            ViewData["IdExcluir"] = id;
            return View();

        }
        public IActionResult ExcluirCliente(int id)
        {
            new ClienteUsuarioModel().Excluir(id);
            return View();
        }

        public IActionResult GerarDadosClientes()
        {
            // Configuração do Faker para gerar clientes falsos
            var clienteFaker = new Faker<ClienteModel>()
                .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
                .RuleFor(c => c.Nome, f => f.Company.CompanyName()) // Gera um nome de empresa
                .RuleFor(c => c.CNPJ, f => f.Random.ReplaceNumbers("##.###.###/####-##")) // Gera um CNPJ no formato brasileiro
                .RuleFor(c => c.End, f => f.Address.StreetAddress()) // Gera um endereço
                .RuleFor(c => c.UF, f => f.Address.StateAbbr()) // Gera um estado brasileiro (UF)
                .RuleFor(c => c.Telefone, f => f.Phone.PhoneNumber()) // Gera um número de telefone
                .RuleFor(c => c.Situacao, f => f.PickRandom(new[] { "ATIVO", "INATIVO", "LEAD" })); // Escolhe aleatoriamente a situação

            // Gera 1000 clientes falsos
            List<ClienteModel> clientesFalsos = clienteFaker.Generate(50);

            // Chama a função Cadastrar para cada cliente gerado (substitua pela lógica correta de inserção)
            foreach (var cliente in clientesFalsos)
            {
                cliente.Insert();

            }

            return Content("1000 clientes falsos foram gerados com sucesso!");
        }


        public ActionResult UploadClientes(IFormFile file, ClienteUsuarioModel model)
        {
            if (file != null && file.Length > 0 && Path.GetExtension(file.FileName).Equals(".csv"))
            {
                var clientes = new List<ClienteUsuarioModel>();
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');

                        var cliente = new ClienteUsuarioModel
                        {
                            Nome = values[0],
                            CNPJ = values[1],
                            End = values[2],
                            UF = values[3],
                            Telefone = values[4],
                            Situacao = values[5]
                        };

                        clientes.Add(cliente);
                    }
                }

                // Salva todos os clientes do CSV no banco de dados
                foreach (var cliente in clientes)
                {
                    cliente.Insert(HttpContext);
                }

                TempData["Mensagem"] = $"{clientes.Count} clientes foram cadastrados com sucesso!";
                return RedirectToAction("Lista");
            }
            else if (ModelState.IsValid)
            {
                // Cadastrar cliente manualmente usando o formulário
                model.Insert(HttpContext);
                TempData["Mensagem"] = "Cliente cadastrado com sucesso!";
                return RedirectToAction("Lista");
            }

            ModelState.AddModelError("", "Por favor, envie um arquivo CSV válido ou preencha o formulário corretamente.");
            return View(model);
        }
    }

}
