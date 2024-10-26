using sistema_crm.Uteis;
using MySql.Data.MySqlClient;
using System.Data;
using MySqlX.XDevAPI;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;


namespace sistema_crm.Models
{
    public class PropostaUsuarioModel
    {

        public string Id { get; set; }

        public string Cliente_id { get; set; }

        public string Vendedor_id { get; set; }

        public string Data { get; set; }

        public string Status { get; set; }

        public string Data_fim { get; set; }

        public DateTime DataDe { get; set; }
        public DateTime DataAte { get; set; }
        public double ValorTotal { get; set; }

        public List<ItemUsuarioModel> Itens { get; set; } = new List<ItemUsuarioModel>();

        public class ItemUsuarioModel
        {
            public string Id { get; set; }

            public int Qtde { get; set; }
            public string Descricao { get; set; }

            public double PrecoUnit { get; set; }

            public int Proposta_id { get; set; }

            
        }

            //Para atender o filtro do relatório


        public List<PropostaUsuarioModel> ListagemPropostas(string DataDe, string DataAte)
        {
            return RetornarListagemPropostas(DataDe, DataAte);
        }

        //Listagem Geral
        public List<PropostaUsuarioModel> ListagemPropostas()
        {
            return RetornarListagemPropostas("1900/01/01", "2200/01/01");
        }


        public List<PropostaUsuarioModel> ListaPropostas(HttpContext httpContext)
        {
            // A lista deve ser do tipo PropostaUsuarioModel, já que estamos lidando com propostas completas
            List<PropostaUsuarioModel> lista = new List<PropostaUsuarioModel>();
            DAL objDAL = new DAL();
            string vendedorId = httpContext.Session.GetString("IdUsuarioLogado");

            // Verifica se o ID não está vazio
            if (string.IsNullOrEmpty(vendedorId))
            {
                // Caso o vendedorId esteja vazio ou nulo, retornar uma lista vazia ou outro tratamento necessário
                return lista;
            }

            string sql = @"SELECT t1.idpropostas, t1.data, t1.status, t2.nomeclientes AS cliente, t3.nomevendedor AS vendedor,t4.qtde, t4.preco_unit, " +
            "SUM(t4.preco_unit * t4.qtde) AS total_itens FROM propostas t1 INNER JOIN clientes t2 ON t1.id_clientes = t2.idclientes " +
            "INNER JOIN vendedor t3 ON t1.id_vendedor = t3.idvendedor INNER JOIN item t4 ON t1.idpropostas = t4.id_proposta " +
            "WHERE t3.idvendedor= @VendedorId GROUP BY t1.idpropostas, t1.data, t1.status, t2.nomeclientes, t3.nomevendedor ";
            DataTable dt = objDAL.RetDataTableProposta(sql, new Dictionary<string, object>
            {
                { "@VendedorId", vendedorId }
            });
            // Adiciona item por item à lista 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Cria um objeto PropostaUsuarioModel
                var proposta = new PropostaUsuarioModel
                {
                    Id = dt.Rows[i]["idpropostas"].ToString(),
                    Status = dt.Rows[i]["status"].ToString(),
                    Cliente_id = dt.Rows[i]["cliente"].ToString(),
                    Vendedor_id = dt.Rows[i]["vendedor"].ToString(),
                    ValorTotal = double.Parse(dt.Rows[i]["total_itens"].ToString()),
                    // Outros campos
                };

                // Aqui você pode preencher os itens de proposta (se necessário)
                proposta.Itens.Add(new PropostaUsuarioModel.ItemUsuarioModel
                {
                 
                    Qtde = int.Parse(dt.Rows[i]["qtde"].ToString()),
                   // Descricao = dt.Rows[i]["descricao"].ToString(),
                    PrecoUnit = double.Parse(dt.Rows[i]["preco_unit"].ToString()),
                  
                   

                });

                // Adiciona a proposta à lista de PropostaUsuarioModel
                lista.Add(proposta);
            }

            return lista;
        }

        public PropostaUsuarioModel RetornarProposta(int? id)
        {

            PropostaUsuarioModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT t1.idpropostas, t1.data, t1.status, t2.nomeclientes AS cliente, t3.nomevendedor AS vendedor,t4.qtde, t4.preco_unit, descricao, " +
            "SUM(t4.preco_unit * t4.qtde) AS total_itens " +
            "FROM propostas t1 " +
            "INNER JOIN clientes t2 ON t1.id_clientes = t2.idclientes " +
            "INNER JOIN vendedor t3 ON t1.id_vendedor = t3.idvendedor " +
            "INNER JOIN item t4 ON t1.idpropostas = t4.id_proposta " 
            + $"WHERE t1.idpropostas = '{id}'";
            DataTable dt = objDAL.RetDataTable(sql);



            // Cria um objeto PropostaUsuarioModel
            item = new PropostaUsuarioModel
            {
                Id = dt.Rows[0]["idpropostas"].ToString(),
                Status = dt.Rows[0]["status"].ToString(),
                Cliente_id = dt.Rows[0]["cliente"].ToString(),
                Vendedor_id = dt.Rows[0]["vendedor"].ToString(),
                ValorTotal = double.Parse(dt.Rows[0]["total_itens"].ToString()),
                // Outros campos
            };

            // Aqui você pode preencher os itens de proposta (se necessário)
            item.Itens.Add(new PropostaUsuarioModel.ItemUsuarioModel
            {

                Qtde = int.Parse(dt.Rows[0]["qtde"].ToString()),
                Descricao = dt.Rows[0]["descricao"].ToString(),
                PrecoUnit = double.Parse(dt.Rows[0]["preco_unit"].ToString()),



            });


            return item;
        }

        public PropostaUsuarioModel ObterPropostaPorId(int? id)
        {
            PropostaUsuarioModel proposta = new PropostaUsuarioModel();
            DAL objDAL = new DAL();

            // Carrega a proposta do banco de dados
            string sql = $"SELECT t1.idpropostas, t1.data, t1.status, t2.nomeclientes AS cliente, t3.nomevendedor AS vendedor,t4.qtde, t4.preco_unit, descricao, " +
            "SUM(t4.preco_unit * t4.qtde) AS total_itens " +
            "FROM propostas t1 " +
            "INNER JOIN clientes t2 ON t1.id_clientes = t2.idclientes " +
            "INNER JOIN vendedor t3 ON t1.id_vendedor = t3.idvendedor " +
            "INNER JOIN item t4 ON t1.idpropostas = t4.id_proposta "
            + $"WHERE t1.idpropostas = '{id}'"; 

            DataTable dt = objDAL.RetDataTable(sql);

            if (dt.Rows.Count > 0)
            {
                proposta.Id = dt.Rows[0]["idpropostas"].ToString();
                proposta.Cliente_id = dt.Rows[0]["cliente"].ToString();
                proposta.Vendedor_id = dt.Rows[0]["vendedor"].ToString();
                proposta.Status = dt.Rows[0]["status"].ToString();

                // Carrega os itens relacionados à proposta
                proposta.Itens = ObterItensDaProposta(proposta.Id);
            }

            return proposta;
        }

        public List<PropostaUsuarioModel.ItemUsuarioModel> ObterItensDaProposta(string propostaId)
        {
            List<PropostaUsuarioModel.ItemUsuarioModel> itens = new List<PropostaUsuarioModel.ItemUsuarioModel>();
            DAL objDAL = new DAL();


            string sql = $"SELECT * FROM item WHERE id_proposta = '{propostaId}'";
            DataTable dt = objDAL.RetDataTable(sql);

            foreach (DataRow row in dt.Rows)
            {
                itens.Add(new PropostaUsuarioModel.ItemUsuarioModel
                {
                    Qtde = Convert.ToInt32(row["qtde"]),
                    Descricao = row["descricao"].ToString(),
                    PrecoUnit = Convert.ToDouble(row["preco_unit"]),
                });
            }

            return itens;
        }



        public List<PropostaUsuarioModel> RetornarListagemPropostas(string DataDe, string DataAte)
        {
            List<PropostaUsuarioModel> lista = new List<PropostaUsuarioModel>();
            PropostaUsuarioModel item;
            DAL objDAL = new DAL();
            string sql = $"Select t1.idpropostas, t1.data, t1.status,t1.data_finalizacao, t2.nomeclientes as cliente, " +
            $"t3.nomevendedor as vendedor from propostas t1 inner join clientes t2 on t1.id_clientes = t2.idclientes inner join vendedor t3 on t1.id_vendedor = t3.idvendedor "
            + $"WHERE t1.data_finalizacao >='{DataDe}' and t1.data_finalizacao <= '{DataAte}' order by data_finalizacao, valor";
            DataTable dt = objDAL.RetDataTable(sql);

            for (int i = 0; i < dt.Rows.Count; i++)
            { 

                item = new PropostaUsuarioModel
                {
                Id = dt.Rows[0]["idpropostas"].ToString(),
                Data = DateTime.Parse(dt.Rows[0]["data"].ToString()).ToString("dd/MM/yyyy"),
                Status = dt.Rows[0]["status"].ToString(),
                Cliente_id = dt.Rows[0]["cliente"].ToString(),
                Vendedor_id = dt.Rows[0]["vendedor"].ToString(),
                Data_fim = DateTime.Parse(dt.Rows[0]["data_finalizacao"].ToString()).ToString("dd/MM/yyyy")
                };
                lista.Add(item);
            }

            return lista;
        }




        public void Gravar()
            {
                 DAL objDAL = new DAL();

                

                string sql = $"INSERT INTO PROPOSTAS(data,  status, id_clientes, id_vendedor ) " +
                $"VALUES( '{Data}', '{Status}','{Cliente_id}','{Vendedor_id}')";

                 
                objDAL.ExecutarComandoSQL(sql);



             }


        

            public void Excluir(int id)
            {
                DAL objDAL = new DAL();
                string sql = $"DELETE FROM PROPOSTAS WHERE idpropostas='{id}'";
                objDAL.ExecutarComandoSQL(sql);
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
