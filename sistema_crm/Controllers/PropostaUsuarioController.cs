using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using sistema_crm.Uteis;
using System.Text;
using Bogus;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlX.XDevAPI;
using X.PagedList;
using Microsoft.EntityFrameworkCore;

namespace sistema_crm.Controllers
{
    public class PropostaUsuarioController : Controller
    {


        [HttpGet]
        public IActionResult DownloadPropostasCsv()
        {
            var propostaModel = new PropostaUsuarioModel();
            var propostas = propostaModel.ListaPropostas(HttpContext); // Obtenha a lista de propostas

            // Gera o CSV
            var csv = new StringBuilder();
            csv.AppendLine("Id,Cliente_id,Vendedor_id,Data,Status,Data_fim,DataDe,DataAte,ValorTotal");

            foreach (var proposta in propostas)
            {
                csv.AppendLine($"{proposta.Id},{proposta.Cliente_id},{proposta.Vendedor_id},{proposta.Data},{proposta.Status},{proposta.Data_fim},{proposta.DataDe:yyyy-MM-dd},{proposta.DataAte:yyyy-MM-dd},{proposta.ValorTotal}");

                // Inclui os itens da proposta
                foreach (var item in proposta.Itens)
                {
                    csv.AppendLine($",Item: {item.Id}, Qtde: {item.Qtde}, Descricao: {item.Descricao}, PrecoUnit: {item.PrecoUnit}");
                }
            }

            // Retorna o arquivo CSV
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "propostas.csv");
        }
        [HttpPost]
        public IActionResult Cadastrar(PropostaUsuarioModel model)
        {
            DAL objDAL = new DAL();

            string sqlProposta = $"INSERT INTO Propostas (id_clientes, id_vendedor, data, status) VALUES ('{model.Cliente_id}', '{model.Vendedor_id}', '{model.Data}', '{model.Status}')";

            // Executa a inserção da proposta
            objDAL.ExecutarComandoSQL(sqlProposta);

            // Recupera o ID da proposta recém-inserida
            string sqlGetLastId = "SELECT LAST_INSERT_ID()";
            int idProposta = objDAL.ExecutarConsultaSQL(sqlGetLastId);

            // Mapeia os itens da ViewModel para o modelo Item
            foreach (var item in model.Itens)
            {
                string sqlItem = $"INSERT INTO item (id_proposta, qtde, descricao, preco_unit) VALUES ('{idProposta}', '{item.Qtde}', '{item.Descricao}', '{item.PrecoUnit}')";
                objDAL.ExecutarComandoSQL(sqlItem);
            }

            return RedirectToAction("Lista", "Proposta");
          
        }

        public IActionResult AtualizarProposta(PropostaUsuarioModel model)
        {
            DAL objDAL = new DAL();

            // Atualiza os dados da proposta
            string sqlProposta = $"UPDATE Propostas SET status = '{model.Status}', data_finalizacao='{model.Data_fim}' WHERE idpropostas = '{model.Id}'";
            objDAL.ExecutarComandoSQL(sqlProposta);

            // Atualiza os itens relacionados à proposta
            foreach (var item in model.Itens)
            {
                // Verifica se o item já existe no banco (pelo ID do item, por exemplo)
                if (item.Id != null)
                {
                    // Atualiza o item existente
                    string sqlItem = $"UPDATE item SET qtde = '{item.Qtde}', descricao = '{item.Descricao}', preco_unit = '{item.PrecoUnit}' WHERE id_item = '{item.Id}'";
                    objDAL.ExecutarComandoSQL(sqlItem);
                }
                else
                {
                    // Se o item não existir (novo item), insere um novo
                    string sqlItem = $"INSERT INTO item ( qtde, descricao, preco_unit) VALUES ( '{item.Qtde}', '{item.Descricao}', '{item.PrecoUnit}')";
                    objDAL.ExecutarComandoSQL(sqlItem);
                }
            }

            return RedirectToAction("Lista", "Proposta");
        }

        public IActionResult CadastrarProposta()
        {
            var model = new PropostaUsuarioModel
            {

                Itens = new List<PropostaUsuarioModel.ItemUsuarioModel>()
            };
            return View("Lista");
        }



        [HttpGet]
        public IActionResult Relatorio()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Relatorio(PropostaUsuarioModel proposta)
        {
            if (proposta.DataDe.Year == 1)
            {
                ViewBag.ListaPropostas = new PropostaUsuarioModel().ListaPropostas(HttpContext);
            }
            else
            {
                string DataDe = proposta.DataDe.ToString("yyyy/MM/dd");
                string DataAte = proposta.DataAte.ToString("yyyy/MM/dd");
                ViewBag.ListaPropostas = new PropostaUsuarioModel().ListagemPropostas(DataDe, DataAte);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Lista(int? page)
        {
           ViewBag.ListaProposta = new PropostaUsuarioModel().ListaPropostas(HttpContext);
           
            /*int pageSize = 5;
            int pageNumber = (page ?? 1);
            var propostas = _context.Propostas.Include(p => p.Itens).ToPagedList(pageSize, pageNumber);*/
            return View();
        }

        [HttpGet]
        public ActionResult Criar()
        {
            var model = new PropostaUsuarioModel(); // Inicialize um modelo vazio
            CarregarDados(); // Carrega os dados de clientes e vendedores
            return View(model); // Passa o modelo para a view
        }

        [HttpPost]
        public IActionResult AdicionarProposta(PropostaUsuarioModel proposta)
        {
            proposta.Gravar();
            CarregarDados();
            return RedirectToAction("Lista", "PropostaUsuario");
        }

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            var proposta = new PropostaUsuarioModel().ObterPropostaPorId(id);

            if (proposta == null)
            {
                return NotFound(); // Caso a proposta não seja encontrada
            }

            if (proposta.Itens == null)
            {
                proposta.Itens = new List<PropostaUsuarioModel.ItemUsuarioModel>();
            }

            ViewBag.Proposta = proposta;
            CarregarDados(); // Carregar listas de clientes e vendedores para a view
            
            return View();
        }

        [HttpPost]
        public IActionResult Editar(PropostaUsuarioModel proposta)
        {
            CarregarDados();
            return RedirectToAction("Lista", "PropostaUsuario");
        }

      

        // Método de upload de CSV e processamento dos dados
        [HttpPost]
        public ActionResult UploadCsv(IFormFile csvFile)
        {
            if (csvFile != null && csvFile.Length > 0)
            {
                try
                {
                    var proposta = new PropostaUsuarioModel();

                    using (var reader = new StreamReader(csvFile.OpenReadStream()))
                    {
                        // Lê o CSV (ajustar conforme necessário)
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        if (values.Length < 8)
                        {
                            ViewBag.Message = "Erro: O arquivo CSV não contém colunas suficientes.";
                            CarregarDados(); // Carregar os dados após erro
                            return View("Criar", proposta);
                        }

                        // Preenche o modelo com os valores do CSV
                        proposta.Id = values[0];
                        proposta.Cliente_id = values[1]; // Cliente_id do CSV
                        proposta.Vendedor_id = values[2]; // Vendedor_id do CSV
                        proposta.Data = values[4]; // Data no formato yyyy-MM-dd ou como string
                        proposta.Status = values[6];
                        proposta.Data_fim = values[7];
                    }

                    // Carregar os dados de clientes e vendedores para a view
                    CarregarDados();
                    return View("Criar", proposta); // Passa o modelo preenchido para a view
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Erro ao processar o arquivo CSV: {ex.Message}";
                    CarregarDados(); // Carregar os dados após erro
                    return View("Criar", new PropostaUsuarioModel());
                }
            }
            else
            {
                ViewBag.Message = "Por favor, selecione um arquivo CSV.";
                CarregarDados(); // Carregar os dados após erro
                return View("Criar", new PropostaUsuarioModel());
            }
        }

        // Método para carregar listas de clientes e vendedores
        private void CarregarDados()
        {
            ViewBag.ListaClientes = new PropostaUsuarioModel().RetornarListaClientes();
            ViewBag.ListaVendedores = new PropostaUsuarioModel().RetornarListaVendedores();
        }
        private List<ClienteModel> ObterClientesExistentes()
        {
            var listaClientes = new PropostaModel().RetornarListaClientes();

            // Verifica se a lista não é nula e retorna a lista
            if (listaClientes != null)
            {
                return listaClientes;
            }

            // Se não houver clientes, retorna uma lista vazia
            return new List<ClienteModel>();

        }
        private List<VendedorModel> ObterVendedoresExistentes()
        {
            // Assumindo que a função RetornarListaVendedores retorna uma lista de VendedorModel
            var listaVendedores = new VendedorModel().RetornarListaVendedores();

            // Verifica se a lista não é nula e retorna a lista
            if (listaVendedores != null)
            {
                return listaVendedores;
            }

            // Se não houver vendedores, retorna uma lista vazia
            return new List<VendedorModel>();
        }

        public IActionResult GerarDadosPropostas()
        {
            var clientesExistentes = ObterClientesExistentes().Select(c => c.Id.ToString()).ToList(); ; // Função para retornar a lista de IDs de clientes
            var vendedoresExistentes = ObterVendedoresExistentes().Select(c => c.Id.ToString()).ToList();  // Função para retornar a lista de IDs de vendedores

            if (!clientesExistentes.Any() || !vendedoresExistentes.Any())
            {
                return Content("Não há clientes ou vendedores suficientes no banco de dados para gerar propostas.");
            }
            // Configuração do Faker para gerar itens de proposta falsos
            var itemFaker = new Faker<PropostaUsuarioModel.ItemUsuarioModel>()
                .RuleFor(i => i.Id, f => Guid.NewGuid().ToString())
                .RuleFor(i => i.Qtde, f => f.Random.Int(1, 10))
                .RuleFor(i => i.Descricao, f => f.Commerce.ProductName())
                .RuleFor(i => i.PrecoUnit, f => f.Random.Double(10, 1000))
                .RuleFor(i => i.Proposta_id, f => f.Random.Int(1, 1000));

            // Configuração do Faker para gerar propostas falsas
            var propostaFaker = new Faker<PropostaUsuarioModel>()
                .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
                .RuleFor(p => p.Cliente_id, (f, p) => f.PickRandom(clientesExistentes)) 
                 .RuleFor(p => p.Vendedor_id, (f, p) => f.PickRandom(vendedoresExistentes))
                .RuleFor(p => p.Data, f => f.Date.Past(1).ToString("dd/MM/yyyy"))
                .RuleFor(p => p.Status, f => f.PickRandom(new[] { "EM NEGOCIAÇÃO", "VENDA REALIZADA", "CANCELADO", "PERDIDO" }))
               .RuleFor(p => p.Data_fim, (f, p) => p.Status == "EM NEGOCIACAO" ? "" : f.Date.Future(1).ToString("dd/MM/yyyy")) // Data_fim vazia se Status for "EM NEGOCIACAO"
                .RuleFor(p => p.DataDe, f => f.Date.Past(1))
                .RuleFor(p => p.DataAte, f => f.Date.Future(1))
                .RuleFor(p => p.ValorTotal, f => f.Random.Double(5000, 50000))
                .RuleFor(p => p.Itens, f => itemFaker.Generate(f.Random.Int(1, 5))); // Gera de 1 a 5 itens para cada proposta

            // Gera 1000 propostas falsas
            List<PropostaUsuarioModel> propostasFalsas = propostaFaker.Generate(5);

            // Aqui você pode inserir as propostas geradas no banco de dados
            foreach (var proposta in propostasFalsas)
            {
                Cadastrar(proposta);

            }

            return Content("1000 propostas falsas foram geradas e salvas no banco de dados com sucesso!");
        }


    }


   
}