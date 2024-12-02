using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using sistema_crm.Uteis;
using System.Data;
using static sistema_crm.Models.VendedorModel;

namespace sistema_crm.Controllers
{
    public class HomeUsuarioController : Controller
    {
        
        public IActionResult Menu(HomeUsuarioModel home, VendedorModel vendedor, AtividadeUsuarioModel activity)
        {
            string userId = HttpContext.Session.GetString("IdUsuarioLogado");

            int id = int.Parse(userId);
           
            
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
                ViewBag.UltimoValorMeta = vendedor.UltimoValorMeta();

                ObterProgressoVendedorVendas(vendedor, id);
                CarregarDados();

                return View();
         }

        [HttpGet]
        public JsonResult ObterProgressoVendedorVendas(VendedorModel vendedor, int id)
        {

            ViewBag.UltimoValorMeta = vendedor.UltimoValorMeta();

            double totalVendas = vendedor.ObterVendasVendedorMes(id);
            double progresso = ViewBag.UltimoValorMeta > 0 ? (totalVendas / (double)ViewBag.UltimoValorMeta) * 100 : 0;
            return Json(new { progresso = progresso, totalVendas = totalVendas, meta = ViewBag.UltimoValorMeta });
        }

        [HttpPost]
        public IActionResult CriarAtividade(AtividadeUsuarioModel atividade, int id)
        {
          

            string userId = HttpContext.Session.GetString("IdUsuarioLogado");

            
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");

            sql = $"INSERT INTO Atividade (contato, dtcontato, observacao, idclientes, idvendedor ) VALUES('{atividade.Tipo_contato}', '{data}', '{atividade.Obs}', '{id}', '{userId}')";



            objDAL.ExecutarComandoSQL(sql);


            return RedirectToAction("Menu", "HomeUsuario");
            
        }

        private void CarregarDados()
        {
            var listaVendedores = new ClienteModel().RetornarListaVendedores();
            ViewBag.ListaVendedores = listaVendedores ?? new List<VendedorModel>();

            var listaClientes = new PropostaModel().RetornarListaClientes();
            ViewBag.ListaClientes = listaClientes ?? new List<ClienteModel>();
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






        public IActionResult Index()
        {
            return View();
        }


    }
}
