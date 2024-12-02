using Bogus;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using sistema_crm.Models;
using sistema_crm.Uteis;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using X.PagedList;
using static sistema_crm.Models.VendedorModel;

namespace sistema_crm.Controllers
{
    public class ClienteController : Controller
    {

        [HttpGet]
        public IActionResult DownloadClientesCsv()
        {
            var clienteModel = new ClienteModel();
            var clientes = clienteModel.ListarClientes(); // Obtenha a lista de clientes

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
        public IActionResult Lista(int? page, string searchString, string situacao = "todos")
        {
            var clientes = new ClienteModel().ListarClientes(situacao);

            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.Nome.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);


            ViewBag.Situacoes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Todos", Value = "todos", Selected = (situacao == "todos") },
                new SelectListItem { Text = "Ativo", Value = "Ativo", Selected = (situacao == "Ativo") },
                new SelectListItem { Text = "Inativo", Value = "Inativo", Selected = (situacao == "Inativo") },
                new SelectListItem { Text = "Lead", Value = "Lead", Selected = (situacao == "Lead") }
            };

            // Mantém o filtro de busca atual
            ViewData["CurrentFilter"] = searchString;

            return View(clientes.ToPagedList(pageNumber, pageSize));

        }

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id != null)
            {
                // Carregar o registro de cliente em uma viewbag

                ViewBag.Cliente = new ClienteModel().RetornarCliente(id);
                CarregarDados();

            }

            return View();
        }

        [HttpGet]
        public IActionResult Criar()
        {
            //  ViewBag.ActivePage = "Cliente";
            CarregarDados();
            return View();


        }


        public IActionResult NomeCliente(ClienteModel cliente, int id)
        {
            DAL objDAL = new DAL();

            // Busca os dados do vendedor
            string sqlCliente = "SELECT * FROM Clientes WHERE idclientes = @id";

            // Adicione o valor do parâmetro `@id`
            var parametrosCliente = new Dictionary<string, object>
            {
                { "@id", id } // 'id' deve ser a variável que armazena o ID do vendedor
            };

            // Executa a query para buscar o vendedor
            DataTable dtCliente = objDAL.RetornarDataTable(sqlCliente, parametrosCliente);

            if (dtCliente.Rows.Count > 0)
            {
                // Mapeia o resultado da query para o modelo `VendedorModel`
                cliente = new ClienteModel
                {
                    Id = dtCliente.Rows[0]["idclientes"].ToString(),
                    Nome = dtCliente.Rows[0]["nomeclientes"].ToString()
                };
            }

            ViewBag.Clientes = cliente;
            return View();
        }

        public IActionResult GraficoVendasClientes(int id)
        {
            List<ClienteModel> lista = new ClienteModel().RetornarGraficoValoresClientes(id);
            ViewBag.RetornarGraficoValoresClientes = lista;

            string valores = "";
            string labels = "";

            // Percorre a lista de itens para compor o gráfico de linha
            for (int i = 0; i < lista.Count; i++)
            {
                valores += lista[i].Valor.ToString() + ",";
                labels += "'" + lista[i].Mes.ToString() + "',";
            }

            ViewBag.Valores = valores.TrimEnd(',');
            ViewBag.Labels = labels.TrimEnd(',');

            return View();

        }

      

        public IActionResult RetornarMediaCliente(ClienteModel cliente, int id)
        {
            DAL objDAL = new DAL();
            string sqlTotalMedia = @"WITH Meses AS (
                    SELECT 1 AS mes_num, 'January' AS mes
                    UNION ALL SELECT 2, 'February'
                    UNION ALL SELECT 3, 'March'
                    UNION ALL SELECT 4, 'April'
                    UNION ALL SELECT 5, 'May'
                    UNION ALL SELECT 6, 'June'
                    UNION ALL SELECT 7, 'July'
                    UNION ALL SELECT 8, 'August'
                    UNION ALL SELECT 9, 'September'
                    UNION ALL SELECT 10, 'October'
                    UNION ALL SELECT 11, 'November'
                    UNION ALL SELECT 12, 'December'
                )
                SELECT 
                    t3.nomeclientes,
                    ROUND(COALESCE(AVG(vendas.total_vendas), 0), 2) AS valor_medio_vendas
                FROM 
                    (SELECT idclientes, nomeclientes FROM clientes WHERE idclientes = @id) t3
                LEFT JOIN (
                    SELECT 
                        t2.id_clientes,
                        SUM(t1.qtde * t1.preco_unit) AS total_vendas,
                        MONTH(t2.data_finalizacao) AS mes_num
                    FROM 
                        item t1
                    JOIN 
                        propostas t2 ON t2.idpropostas = t1.id_proposta
                    WHERE 
                        t2.status = 'VENDA REALIZADA'
                    GROUP BY 
                        t2.id_clientes, MONTH(t2.data_finalizacao)
                ) vendas ON t3.idclientes = vendas.id_clientes
                GROUP BY 
                    t3.nomeclientes;";



            // Parâmetros SQL para o total de vendas
            var parametrosMedia = new Dictionary<string, object>
                {
                    { "@id", id }
                };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dtNegociacao = objDAL.RetornarDataTable(sqlTotalMedia, parametrosMedia);

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
                if (dtNegociacao.Columns.Contains("valor_medio_vendas") && dtNegociacao.Rows[0]["valor_medio_vendas"] != DBNull.Value)
                {

                    totalNegociacao = Convert.ToDouble(dtNegociacao.Rows[0]["valor_medio_vendas"]);
                }
                else
                {
                    // Se a coluna não existir ou for nula, totalVendas será 0
                    totalNegociacao = 0;
                    Console.WriteLine("Nenhuma venda realizada ou coluna 'valor_medio_vendas' não encontrada.");
                }
            }
            else
            {
                Console.WriteLine("Nenhuma linha retornada.");
            }

            ViewBag.TotalMedia = totalNegociacao;

            return View();
        }

        public IActionResult RetornarPropostasCliente(ClienteModel cliente, int id)
        {

            DAL objDAL = new DAL();
            string sql = @"SELECT 
                    c.nomeclientes AS cliente,
                    COUNT(*) AS total_propostas_realizadas,
                    SUM(CASE WHEN p.status = 'VENDA REALIZADA' THEN 1 ELSE 0 END) AS total_propostas_vendidas
                FROM 
                    propostas p
                JOIN 
                    clientes c ON p.id_clientes = c.idclientes
                WHERE 
                    c.idclientes = @id

                GROUP BY 
                    c.nomeclientes;";



            // Parâmetros SQL para o total de vendas
            var parametrosMedia = new Dictionary<string, object>
                {
                    { "@id", id }
                };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dt = objDAL.RetornarDataTable(sql, parametrosMedia);



            List<object> propostasData = new List<object>();

            if (dt.Rows.Count > 0)
            {
                // Itera sobre as linhas retornadas para construir a lista de objetos
                foreach (DataRow row in dt.Rows)
                {
                    var propostaInfo = new
                    {
                        Cliente = row["cliente"]?.ToString(),
                        TotalPropostasRealizadas = Convert.ToInt32(row["total_propostas_realizadas"]),
                        TotalPropostasVendidas = Convert.ToInt32(row["total_propostas_vendidas"])
                    };

                    propostasData.Add(propostaInfo);
                }
            }
            else
            {
                Console.WriteLine("Nenhuma informação de propostas encontrada para o cliente.");
            }

            // Passa as informações separadas para a ViewBag
            ViewBag.PropostasData = propostasData;
            return View();
        }

        [HttpGet]
        public IActionResult DetalhesCliente(int id, ClienteModel cliente)
        {
            NomeCliente(cliente, id);
            GraficoVendasClientes(id);
            RetornarMediaCliente(cliente, id);
            RetornarPropostasCliente(cliente, id);
            return View();
        }



        [HttpPost]
        public IActionResult Criar(ClienteModel cliente)
        {
            if (ModelState.IsValid)
            {
                // Retorna para a view com os erros de validação
                return View();
            }
            if (!ValidarCNPJ(cliente.CNPJ))
            {
                ModelState.AddModelError("CNPJ", "CNPJ inválido");
                CarregarDados();
                return View();
            }

            if (cliente.CnpjExiste(cliente.CNPJ))
            {
                ModelState.AddModelError("CNPJ", "Cliente existente");
                CarregarDados();
                return View();
            }

            cliente.Gravar();
            
            TempData["SuccessMessage"] = "Cliente cadastrado com sucesso!";
            return RedirectToAction("Lista", "Cliente");


        }


        public JsonResult VerificarCNPJ(string cnpj, ClienteModel cliente)
        {
            bool clienteExiste = cliente.CnpjExiste(cnpj);

            if (clienteExiste)
            {
                return Json(new { existe = true, mensagem = "Cliente existente" });
            }
            return Json(new { existe = false, mensagem = "" });
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
        public IActionResult Editar(ClienteModel cliente)
        {

            CarregarDados();
            cliente.Gravar();
            return RedirectToAction("Lista", "Cliente");




        }


        public IActionResult Excluir(int id)
        {
            ViewData["IdExcluir"] = id;
            return View();

        }
        public IActionResult ExcluirCliente(int id)
        {
            new ClienteModel().Excluir(id);
            return View();
        }

        private void CarregarDados()
        {
            var listaVendedores = new ClienteModel().RetornarListaVendedores();
            ViewBag.ListaVendedores = listaVendedores ?? new List<VendedorModel>();
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


        public ActionResult UploadClientes(IFormFile file, ClienteModel model)
        {
            if (file != null && file.Length > 0 && Path.GetExtension(file.FileName).Equals(".csv"))
            {
                var clientes = new List<ClienteModel>();
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');

                        var cliente = new ClienteModel
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
                    cliente.Insert();
                }

                TempData["Mensagem"] = $"{clientes.Count} clientes foram cadastrados com sucesso!";
                return RedirectToAction("Lista");
            }
            else if (ModelState.IsValid)
            {
                // Cadastrar cliente manualmente usando o formulário
                model.Insert();
                TempData["Mensagem"] = "Cliente cadastrado com sucesso!";
                return RedirectToAction("Lista");
            }

            ModelState.AddModelError("", "Por favor, envie um arquivo CSV válido ou preencha o formulário corretamente.");
            return View(model);
        }

       
    }

}
