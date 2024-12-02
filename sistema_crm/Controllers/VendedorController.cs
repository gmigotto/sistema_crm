using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using Microsoft.EntityFrameworkCore;
using sistema_crm.Uteis;
using static sistema_crm.Models.VendedorModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using System.Data;
using MySqlX.XDevAPI;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grpc.Core;


namespace sistema_crm.Controllers
{
    public class VendedorController : Controller
    {
        private readonly VendedorModel _context;

        private const double MetaVendas = 100000;

        [HttpGet]
        public IActionResult DownloadVendedoresCsv()
        {
            var vendedorModel = new VendedorModel();
            var vendedores = vendedorModel.ListarVendedores(); // Obtenha a lista de vendedores

            // Gera o CSV
            var csv = new StringBuilder();
            csv.AppendLine("Id,Nome,CPF,Nascimento,Telefone,Endereco,Email,Status,DataADM");

            foreach (var vendedor in vendedores)
            {
                csv.AppendLine($"{vendedor.Id},{vendedor.Nome},{vendedor.CPF},{vendedor.Nascimento},{vendedor.Telefone},{vendedor.Endereco},{vendedor.Email},{vendedor.Status},{vendedor.DataADM}");
            }

            // Retorna o arquivo CSV
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "vendedores.csv");
        }


        public VendedorController()
        {
            _context = new VendedorModel();
        }

        [HttpGet]
        public ActionResult Meta(VendedorModel vendedor)
        {
            var ultimaMeta = vendedor.UltimaMeta();
            ViewBag.UltimaMeta = ultimaMeta;

            return View();
        }

        [HttpPost]
        public ActionResult Meta(VendedorModel vendedor,  string valorMeta, string dataInicio, string dataFim)
        {
            ViewBag.AdminId = HttpContext.Session.GetString("IdAdminLogado");


            vendedor.CriarMeta(HttpContext);

                // Buscar a última meta inserida
                var ultimaMeta = vendedor.UltimaMeta();
                ViewBag.UltimaMeta = ultimaMeta;

                ViewBag.Mensagem = "Meta cadastrada com sucesso!";
     
            return View();

        }

        


        public IActionResult Lista(int? page, string searchString, string status = "todos")
        {
            var vendedor = new VendedorModel().ListarVendedores(status);
            // ViewBag.ListaVendedores = new VendedorModel().ListarVendedores();

           

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            if (!string.IsNullOrEmpty(searchString))
            {
                vendedor = vendedor.Where(c => c.Nome.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Situacoes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todos", Value = "todos", Selected = (status == "todos") },
                new SelectListItem { Text = "Ativo", Value = "Ativo", Selected = (status == "Ativo") },
                new SelectListItem { Text = "Inativo", Value = "Inativo", Selected = (status == "Inativo") },

            };

            ViewData["CurrentFilter"] = searchString;
            return View(vendedor.ToPagedList(pageNumber, pageSize));

        }


        [HttpGet]
        public JsonResult ObterProgressoVendas(VendedorModel vendedor)
        {
            int qtdVendedoresAtivos = vendedor.ObterQuantidadeVendedoresAtivos(); 
            
            ViewBag.UltimoValorMeta = vendedor.UltimoValorMeta();

            double MetaGeral = ViewBag.UltimoValorMeta * qtdVendedoresAtivos;

            double totalVendas = vendedor.ObterVendasDoMes();
            double progresso = MetaGeral> 0 ? (totalVendas / (double)MetaGeral) * 100 : 0;
            return Json(new { progresso = progresso, totalVendas = totalVendas, meta = MetaGeral });

        }


        [HttpGet]
        public JsonResult ObterProgressoVendedorVendas(VendedorModel vendedor, int id)
        {
            ViewBag.UltimoValorMeta = vendedor.UltimoValorMeta();

            double totalVendas = vendedor.ObterVendasVendedorMes(id);
            double progresso = ViewBag.UltimoValorMeta > 0 ? (totalVendas / (double)ViewBag.UltimoValorMeta) * 100 : 0;
            return Json(new { progresso = progresso, totalVendas = totalVendas, meta = ViewBag.UltimoValorMeta });
        }

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id != null)
            {
                // Caregar o registro de cliente em uma viewbag

                ViewBag.Vendedor = new VendedorModel().RetornarVendedor(id);
            }

            return View();
        }

        [HttpPost]
        public IActionResult Editar(VendedorModel vendedor, int? id)
        {
            vendedor.Update();
            return RedirectToAction("Lista", "Vendedor");


        }

        [HttpGet]
        public IActionResult Criar()
        {
            
                //Carregar o registro do vendedor numa ViewBag
                //ViewBag.Vendedor = new VendedorModel().RetornarVendedor(id);
            
            return View();
        }

        [HttpPost]
        public IActionResult Criar(VendedorModel vendedor)
        {
            ViewBag.AdminId = HttpContext.Session.GetString("IdAdminLogado");

            // Valida os campos usando a função ValidarCampos
            var erros = ValidarCampos(vendedor);

            if (erros.Any())
            {
                // Adiciona as mensagens de erro ao ViewBag para exibir na view
                ViewBag.Erros = erros;
                return View(vendedor); // Retorna o vendedor preenchido para que o usuário veja o que está faltando
            }


            // Valida o CPF do vendedor
            if (!ValidarCPF(vendedor.CPF))
            {
                ModelState.AddModelError("CPF", "CPF inválido");
                return View(vendedor); // Retorna a mesma view com o erro de CPF
            }

            try
            {
                // Salva o vendedor no banco de dados
                vendedor.Gravar(HttpContext);

                // Exibe mensagem de sucesso e redireciona para a lista de vendedores
                TempData["SuccessMessage"] = "Vendedor cadastrado com sucesso!";
                return RedirectToAction("Lista", "Vendedor");
            }
            catch (Exception ex)
            {
                // Exibe mensagem de erro e retorna à mesma view
                ModelState.AddModelError(string.Empty, $"Erro ao salvar vendedor: {ex.Message}");
                return View(vendedor);
            }
        }

        private List<string> ValidarCampos(VendedorModel vendedor)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(vendedor.Nome))
                erros.Add("O campo Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.CPF))
                erros.Add("O campo CPF é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.Nascimento))
                erros.Add("O campo Data de Nascimento é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.Telefone))
                erros.Add("O campo Telefone é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.Endereco))
                erros.Add("O campo Endereço é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.Email))
                erros.Add("Informe o e-mail do vendedor.");

            if (string.IsNullOrWhiteSpace(vendedor.Senha))
                erros.Add("O campo Senha é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.Status))
                erros.Add("O campo Status é obrigatório.");

            if (string.IsNullOrWhiteSpace(vendedor.DataADM))
                erros.Add("O campo Data de Admissão é obrigatório.");

            return erros;
        }



        public bool ValidarCPF(string CPF)
        {
            CPF = Regex.Replace(CPF, @"[^\d]", "");

            // Verifica se o CPF possui 11 dígitos após a remoção de caracteres não numéricos
            if (CPF.Length != 11)
            {
                return false;
            }

            // Verifica CPFs com todos os dígitos iguais (111.111.111-11, etc.)
            if (IsSameDigits(CPF))
            {
                return false;
            }

            // Calcula e verifica o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(CPF[i].ToString()) * (10 - i);
            }
            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            // Verifica o primeiro dígito verificador
            if (int.Parse(CPF[9].ToString()) != digit1)
            {
                return false;
            }

            // Calcula e verifica o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += int.Parse(CPF[i].ToString()) * (11 - i);
            }
            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            // Verifica o segundo dígito verificador
            if (int.Parse(CPF[10].ToString()) != digit2)
            {
                return false;
            }

            return true;
        }

        private static bool IsSameDigits(string CPF)
        {
            for (int i = 1; i < CPF.Length; i++)
            {
                if (CPF[i] != CPF[0])
                {
                    return false;
                }
            }
            return true;
        }
        public IActionResult UpdateController(VendedorModel vendedor)
        {
            vendedor.Update();
            return RedirectToAction("Lista", "Vendedor");
        }

        public IActionResult Excluir(int id)
        {
            ViewData["IdExcluir"] = id;
            return View();
        }

        public IActionResult ExcluirVendedor(int id)
        {
            new VendedorModel().Excluir(id);
            return View();
        }

        // Action que gera 5000 vendedores falsos
        public IActionResult GerarDados()
        {
            // Configuração do Faker para gerar vendedores falsos
            var vendedorFaker = new Faker<VendedorModel>()
                .RuleFor(v => v.Id, f => f.Random.Int(1, 1000))
                .RuleFor(v => v.Nome, f => f.Person.FullName)
                .RuleFor(v => v.CPF, f => f.Random.ReplaceNumbers("###.###.###-##"))
                .RuleFor(v => v.Nascimento, f => f.Date.Past(40, DateTime.Now.AddYears(-18)).ToString("dd/MM/yyyy"))
                .RuleFor(v => v.Telefone, f => f.Phone.PhoneNumber())
                .RuleFor(v => v.Endereco, f => f.Address.FullAddress())
                .RuleFor(v => v.Email, f => f.Internet.Email())
                .RuleFor(v => v.Senha, f => f.Internet.Password())
                .RuleFor(v => v.Status, f => f.PickRandom(new[] { "Ativo", "Inativo" }))
                .RuleFor(v => v.DataADM, f => f.Date.Past(10).ToString("dd/MM/yyyy"));


            // Gera 5000 vendedores falsos
            List<VendedorModel> vendedoresFalsos = vendedorFaker.Generate(10);

            foreach (var vendedor in vendedoresFalsos)
            {
                vendedor.Insert();
            }


            return Content("5.000 vendedores falsos foram gerados e salvos no banco de dados com sucesso!");
        }


        [HttpPost]
        public IActionResult RetornoAtividade(VendedorModel vendedor, int id)

        {
            List<AtividadeModel> atividades = new List<AtividadeModel>();
            List<AtividadeModel> vendas = new List<AtividadeModel>();

            DAL objDAL = new DAL();

            // Busca os dados do vendedor
            string sqlVendedor = "SELECT * FROM Vendedor WHERE idvendedor = @id";

            // Adicione o valor do parâmetro `@id`
            var parametrosVendedor = new Dictionary<string, object>
            {
                { "@id", id } // 'id' deve ser a variável que armazena o ID do vendedor
            };

            // Executa a query para buscar o vendedor
            DataTable dtVendedor = objDAL.RetornarDataTable(sqlVendedor, parametrosVendedor);

            if (dtVendedor.Rows.Count > 0)
            {
                // Mapeia o resultado da query para o modelo `VendedorModel`
                vendedor = new VendedorModel
                {
                    Id = Convert.ToInt32(dtVendedor.Rows[0]["idvendedor"]),
                    Nome = dtVendedor.Rows[0]["nomevendedor"].ToString()
                };
            }

            // Busca as atividades do vendedor
            string sqlAtividade = "SELECT * FROM Atividade WHERE idvendedor = @id";

            // Adicione o valor do parâmetro `@id`
            var parametrosAtividade = new Dictionary<string, object>
            {
                { "@id", id } // 'id' deve ser a variável que armazena o ID do vendedor
            };

            // Executa a query para buscar as atividades do vendedor
            DataTable dtAtividades = objDAL.RetornarDataTable(sqlAtividade, parametrosAtividade);

            if (dtAtividades.Rows.Count > 0)
            {
                // Mapeia os resultados da query para o modelo `AtividadeModel`
                foreach (DataRow row in dtAtividades.Rows)
                {
                    DateTime dataContato;
                    bool conversaoValida = DateTime.TryParse(row["dtcontato"].ToString(), out dataContato);

                    var atividade = new AtividadeModel
                    {
                        Id = Convert.ToInt32(row["idatividade"]),
                        Obs = row["observacao"].ToString(),
                        Tipo_contato = row["contato"].ToString(),
                        DT_contato = dataContato.ToString(),
                    };

                    atividades.Add(atividade);
                }
            }

            string sqlTotalVendas = "SELECT SUM(t1.qtde * t1.preco_unit) AS total_vendas " +
                        "FROM item t1 " +
                        "JOIN propostas t2 ON t1.id_proposta = t2.idpropostas " +
                        "WHERE t2.status = 'VENDA REALIZADA' " +
                        "AND MONTH(t2.data_finalizacao) = MONTH(CURDATE()) " +
                        "AND YEAR(t2.data_finalizacao) = YEAR(CURDATE()) " +
                        "AND t2.id_vendedor = @id;";

            // Parâmetros SQL para o total de vendas
            var parametrosVendas = new Dictionary<string, object>
            {
                { "@id", id }
            };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dtVendas = objDAL.RetornarDataTable(sqlTotalVendas, parametrosVendas);

            double totalVendas = 0; // Variável para armazenar o total de vendas


            if (dtAtividades.Rows.Count > 0)
            {
                // Mapeia os resultados da query para o modelo `AtividadeModel`
                foreach (DataRow row in dtAtividades.Rows)
                {
                    DateTime dataContato;
                    bool conversaoValida = DateTime.TryParse(row["dtcontato"].ToString(), out dataContato);

                    var atividade = new AtividadeModel
                    {
                        Id = Convert.ToInt32(row["idatividade"]),
                        Obs = row["observacao"].ToString(),
                        Tipo_contato = row["contato"].ToString(),
                        DT_contato = dataContato.ToString(),
                    };

                    atividades.Add(atividade);
                }
            }
            // Verifica se há resultados
            if (dtVendas.Rows.Count > 0)
            {
                // Log para verificar as colunas retornadas
                foreach (DataColumn column in dtVendas.Columns)
                {
                    Console.WriteLine("Coluna: " + column.ColumnName); // Verifica se 'total_vendas' está presente
                }

                // Verifica se a coluna 'total_vendas' está presente e não é nula
                if (dtVendas.Columns.Contains("total_vendas") && dtVendas.Rows[0]["total_vendas"] != DBNull.Value)
                {

                    totalVendas = Convert.ToDouble(dtVendas.Rows[0]["total_vendas"]);
                }
                else
                {
                    // Se a coluna não existir ou for nula, totalVendas será 0
                    totalVendas = 0;
                    Console.WriteLine("Nenhuma venda realizada ou coluna 'total_vendas' não encontrada.");
                }
            }
            else
            {
                Console.WriteLine("Nenhuma linha retornada.");
            }

            // Passar o total de vendas para a view
            ViewBag.TotalVendas = totalVendas;

            // Passar o vendedor, atividades e total de vendas para a view
            ViewBag.Vendedor = vendedor;
            ViewBag.Atividades = atividades ?? new List<AtividadeModel>();
            GraficoVendasVendedor(id);
            ObterProgressoVendedorVendas(vendedor, id);

            return View();
        }

        [HttpPost]
        public IActionResult RetornoNegociacao(VendedorModel vendedor, int id)
        {
            DAL objDAL = new DAL();
            string sqlTotalNegociacao = "SELECT SUM(t1.qtde * t1.preco_unit) AS total_negociacao FROM item t1 JOIN propostas t2 on t2.idpropostas = t1.id_proposta WHERE t2.status='EM NEGOCIAÇÃO' AND t2.id_vendedor = @id;";


            // Parâmetros SQL para o total de vendas
            var parametrosNegociacao = new Dictionary<string, object>
                {
                    { "@id", id }
                };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dtNegociacao = objDAL.RetornarDataTable(sqlTotalNegociacao, parametrosNegociacao);

            double totalNegociacao = 0; // Variável para armazenar o total de vendas



            // Verifica se há resultados
            if (dtNegociacao.Rows.Count > 0)
            {
                // Log para verificar as colunas retornadas
                foreach (DataColumn column in dtNegociacao.Columns)
                {
                    Console.WriteLine("Coluna: " + column.ColumnName); // Verifica se 'total_vendas' está presente
                }

                // Verifica se a coluna 'total_vendas' está presente e não é nula
                if (dtNegociacao.Columns.Contains("total_negociacao") && dtNegociacao.Rows[0]["total_negociacao"] != DBNull.Value)
                {

                    totalNegociacao = Convert.ToDouble(dtNegociacao.Rows[0]["total_negociacao"]);
                }
                else
                {
                    // Se a coluna não existir ou for nula, totalVendas será 0
                    totalNegociacao = 0;
                    Console.WriteLine("Nenhuma venda realizada ou coluna 'total_negociacao' não encontrada.");
                }
            }
            else
            {
                Console.WriteLine("Nenhuma linha retornada.");
            }

            ViewBag.TotalNegociacao = totalNegociacao;

            return View();
        }

        public IActionResult GraficoVendasVendedor(int id)
        {
             List<RelatorioModel> lista = new RelatorioModel().RetornarGraficoValoresVendedor(id);
                ViewBag.RetornarGraficoValoresVendedor = lista;

                string valores = "";
                string labels = "";

                // Percorre a lista de itens para compor o gráfico de linha
                for (int i = 0; i < lista.Count; i++)
                {
                    valores += lista[i].Valor.ToString() + ",";
                    labels += "'" + lista[i].Mes.ToString() + "',";
                }

                ViewBag.Valores2 = valores.TrimEnd(',');
                ViewBag.Labels2 = labels.TrimEnd(',');

                return View();

        }


        public IActionResult AtividadeVendedor(int id, VendedorModel vendedor)
        {
            RetornoAtividade(vendedor, id);
            RetornoNegociacao(vendedor, id);

            return View();
        }




        public ActionResult UploadVendedores(IFormFile file, VendedorModel model)
        {
            if (file != null && file.Length > 0 && Path.GetExtension(file.FileName).Equals(".csv"))
            {
                var vendedores = new List<VendedorModel>();
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');

                        var vendedor = new VendedorModel
                        {
                            Nome = values[0],
                            CPF = values[1],
                            Nascimento = values[2],
                            Telefone = values[3],
                            Endereco = values[4],
                            Email = values[5],
                            Senha = values[6],
                            Status = values[7],
                            DataADM = values[8]
                        };

                        vendedores.Add(vendedor);
                    }
                }

                // Salva todos os vendedores do CSV no banco de dados
                foreach (var vendedor in vendedores)
                {
                    vendedor.Insert(); // Supondo que você já tenha um método Insert no modelo de Vendedor
                }

                TempData["Mensagem"] = $"{vendedores.Count} vendedores foram cadastrados com sucesso!";
                return RedirectToAction("Lista");
            }
            else if (ModelState.IsValid)
            {
                // Cadastrar vendedor manualmente usando o formulário
                model.Insert();
                TempData["Mensagem"] = "Vendedor cadastrado com sucesso!";
                return RedirectToAction("Lista");
            }

            ModelState.AddModelError("", "Por favor, envie um arquivo CSV válido ou preencha o formulário corretamente.");
            return View(model);
        }



    }
}