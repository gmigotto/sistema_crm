using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using static sistema_crm.Models.VendedorModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using MySql.Data.MySqlClient;
using Grpc.Core;
using static iTextSharp.text.pdf.AcroFields;
using System.Text.RegularExpressions;

namespace sistema_crm.Models
{
    public class VendedorModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Informe nome do vendedor")]
        public string Nome { get; set; }


        public string CPF { get; set; }

        public string Nascimento { get; set; }


        public string Telefone { get; set; }

        public string Endereco { get; set; }

        [Required(ErrorMessage = "Informe o e-mail do vendedor")]

        public string Email { get; set; }
        public string Senha { get; set; }

        public string Status { get; set; }

        public string DataADM { get; set; }

        public double ValorVendas { get; set; }

        

        public virtual ICollection<AtividadeModel> Atividades { get; set; }
        public class AtividadeModel
        {
            public int Id { get; set; }

            public string Obs { get; set; }

            public string Idcliente { get; set; }

            public int Idvendedor { get; set; }

            public string Tipo_contato { get; set; }

            public string DT_contato { get; set; }

            public string Vendedor { get; set; }


        }

        public List<VendedorModel> Vendedores { get; set; } = new List<VendedorModel>();


        public List<VendedorModel> ListarVendedores()
        {
            List<VendedorModel> lista = new List<VendedorModel>();

            VendedorModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT * FROM Vendedor order by nomevendedor desc";
            DataTable dt = objDAL.RetDataTable(sql);

            //Adiciona item por item a lista 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new VendedorModel
                {
                    Id = Convert.ToInt32(dt.Rows[i]["idvendedor"]),
                    Nome = dt.Rows[i]["nomevendedor"].ToString(),
                    CPF = dt.Rows[i]["cpf"].ToString(),
                    Nascimento = dt.Rows[i]["nascimento"].ToString(),
                    Email = dt.Rows[i]["email"].ToString(),
                    Senha = dt.Rows[i]["senha"].ToString(),
                    Status = dt.Rows[i]["status"].ToString(),
                    DataADM = dt.Rows[i]["data_admicao"].ToString()
                };
                lista.Add(item);
            }
            return lista;
        }

        public VendedorModel RetornarVendedor(int? id)
        {

            VendedorModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT idvendedor, nomevendedor, nascimento, cpf, telefone, endereco, email, senha, status, data_admicao FROM Vendedor WHERE idvendedor = '{id}' order by nomevendedor desc";
            DataTable dt = objDAL.RetDataTable(sql);



            item = new VendedorModel
            {
                Id = Convert.ToInt32(dt.Rows[0]["idvendedor"]),
                Nome = dt.Rows[0]["nomevendedor"].ToString(),
                Nascimento = !string.IsNullOrEmpty(dt.Rows[0]["nascimento"].ToString()) ? DateTime.Parse(dt.Rows[0]["nascimento"].ToString()).ToString("dd/MM/yyyy") : null,
                CPF = dt.Rows[0]["cpf"].ToString(),
                Telefone = dt.Rows[0]["telefone"].ToString(),
                Endereco = dt.Rows[0]["endereco"].ToString(),
                Email = dt.Rows[0]["email"].ToString(),
                Senha = dt.Rows[0]["senha"].ToString(),
                Status = dt.Rows[0]["status"].ToString(),
                DataADM = !string.IsNullOrEmpty(dt.Rows[0]["data_admicao"].ToString()) ? DateTime.Parse(dt.Rows[0]["data_admicao"].ToString()).ToString("dd/MM/yyyy") : null

            };

            return item;
        }

        public List<VendedorModel> ObterTop5Vendedores()
        {
            DAL objDAL = new DAL();
            string sql = $"SELECT t1.nomevendedor AS Vendedor, SUM(t2.preco_unit * t2.qtde) AS ValorTotalVendas FROM Vendedor t1 INNER JOIN Propostas p ON t1.idvendedor = p.id_vendedor INNER JOIN Item t2 ON p.idpropostas = t2.id_proposta " +
            "WHERE p.status = 'VENDA REALIZADA' GROUP BY t1.nomevendedor ORDER BY ValorTotalVendas DESC;";
            DataTable dt = objDAL.RetDataTable(sql);

            var ranking = new List<VendedorModel>();

            foreach (DataRow row in dt.Rows)
            {
                ranking.Add(new VendedorModel
                {
                    Nome = row["Vendedor"].ToString(),
                    ValorVendas = Convert.ToDouble(row["ValorTotalVendas"])
                });
            }

            return ranking;

        }

        public void Gravar()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");
            string nomeVendedor = Nome.Replace("'", "''");
            string enderecoVendedor = Endereco.Replace("'", "''");
            string emailVendedor = Email.Replace("'", "''");
            

            sql = $"INSERT INTO Vendedor (nomevendedor, nascimento, cpf, telefone, endereco, email, senha, status, data_admicao) VALUES('{nomeVendedor}', '{Nascimento}', '{CPF}', '{Telefone}', '{enderecoVendedor}', '{emailVendedor}', '{Senha}', '{Status}', '{DataADM}')";



            objDAL.ExecutarComandoSQL(sql);


        }


        public void Insert()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");
           


            sql = $"INSERT INTO Vendedor (nomevendedor, nascimento, cpf, telefone, endereco, email, senha, status, data_admicao) VALUES('{Nome}', '{Nascimento}', '{CPF}', '{Telefone}', '{Endereco}', '{Email}', '{Senha}', '{Status}', '{DataADM}')";



            objDAL.ExecutarComandoSQL(sql);


        }


        public void Update()
        {

            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");


            if (Id != null)
            {
                sql = $"UPDATE Vendedor SET nomevendedor='{Nome}',nascimento='{Nascimento}',cpf ='{CPF}',endereco='{Endereco}',email='{Email}',senha='{Senha}',status='{Status}',data_admicao='{DataADM}' WHERE idvendedor='{Id}'";
            }


            try
            {
                objDAL.ExecutarComandoSQL(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }




        public void Excluir(int id)
        {
            DAL objDAL = new DAL();
            string sql = $"DELETE FROM Vendedor WHERE idvendedor='{id}'";
            objDAL.ExecutarComandoSQL(sql);
        }

        public List<AtividadeModel> ListarAtividades()
        {
            List<AtividadeModel> lista = new List<AtividadeModel>();

            AtividadeModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT * FROM Atividade order by dtcontato desc";
            DataTable dt = objDAL.RetDataTable(sql);

            //Adiciona item por item a lista 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new AtividadeModel
                {
                    Id = Convert.ToInt32(dt.Rows[0]["idatividade"]),
                    Tipo_contato = dt.Rows[i]["contato"].ToString(),
                    DT_contato = DateTime.Parse(dt.Rows[i]["dtcontato"].ToString()).ToString(),
                    Obs = dt.Rows[i]["observacao"].ToString(),
                    Idcliente = dt.Rows[i]["cliente"].ToString(),
                    Idvendedor = Convert.ToInt32(dt.Rows[0]["vendedor"]),

                };
                lista.Add(item);
            }
            return lista;
        }

        public void CriarAtividade(AtividadeModel atividade)
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");

            sql = $"INSERT INTO Atividade (contato, dtcontato, observacao, idclientes, idvendedor ) VALUES('{atividade.Tipo_contato}', '{atividade.DT_contato}', '{atividade.Obs}', '{atividade.Idcliente}', '{atividade.Idvendedor}')";



            objDAL.ExecutarComandoSQL(sql);

        }

        public AtividadeModel RetornarAtividades(int? id)
        {

            AtividadeModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT t1.idatividade, t1.contato, t1.dtcontato, t1.observacao, t2.nomeclientes as cliente, t3.nomevendedor as vendedor FROM Atividade t1 INNER JOIN Clientes t2 ON t1.idatividade = t2.idclientes" +
                         "INNER JOIN Vendedor t3 ON t1.idvendedor = t3.id_vendedor WHERE t1.idatividade= '{id}'";
            DataTable dt = objDAL.RetDataTable(sql);


            item = new AtividadeModel
            {
                Id = Convert.ToInt32(dt.Rows[0]["idatividade"]),
                Tipo_contato = dt.Rows[0]["contato"].ToString(),
                DT_contato = DateTime.Parse(dt.Rows[0]["dtcontato"].ToString()).ToString(),
                Obs = dt.Rows[0]["observacao"].ToString(),
                Idcliente = dt.Rows[0]["cliente"].ToString(),
                Idvendedor = Convert.ToInt32(dt.Rows[0]["vendedor"]),

            };

            return item;
        }

        public double ObterVendasVendedorMes(int id)
        {
            DAL objDAL = new DAL();


            string sql = "SELECT SUM(t1.qtde * t1.preco_unit) AS total_vendas " +
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
           DataTable dtVendas = objDAL.RetornarDataTable(sql, parametrosVendas);

            double totalVendas = 0;

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

            return totalVendas ;
        }

        public double ObterVendasDoMes()
        {
            DAL objDAL = new DAL();


            string sql = $"SELECT SUM(t1.qtde * t1.preco_unit) AS total_negociacao FROM item t1 JOIN propostas t2 ON t1.id_proposta = t2.idpropostas " +
                "WHERE t2.status = 'VENDA REALIZADA' AND MONTH(t2.data) = MONTH(CURDATE()) AND YEAR(t2.data) = YEAR(CURDATE());";

            // Executar o comando SQL e obter o resultado
            double totalVendas = objDAL.RetornarDado(sql);

            return totalVendas;
        }

        public double VendasVendedor(int id)
        {
            DAL objDAL = new DAL();


            string sql = "SELECT SUM(t1.qtde * t1.preco_unit) AS total_vendas " +
                        "JOIN propostas t2 ON t1.id_proposta = t2.idpropostas " +
                        "WHERE t2.status = 'VENDA REALIZADA' " +
                        "AND MONTH(t2.data) = MONTH(CURDATE()) " +
                        "AND YEAR(t2.data) = YEAR(CURDATE()) " +
                        "AND t2.id_vendedor = @id;";

            // Executar o comando SQL e obter o resultado
            double vendedorVendas = objDAL.RetornarDado(sql);

            return vendedorVendas;
        }

        

        public List<ClienteModel> RetornarListaClientes()
        {
            return new ClienteModel().ListarClientes();
        }

        public List<VendedorModel> RetornarListaVendedores()
        {
            return new VendedorModel().ListarVendedores();
        }

    }
}


