using sistema_crm.Uteis;
using MySql.Data.MySqlClient;
using System.Data;
using MySqlX.XDevAPI;
using System.Text.RegularExpressions;


namespace sistema_crm.Models
{
    public class PropostaModel
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

        public List<ItemModel> Itens { get; set; } = new List<ItemModel>();

        public class ItemModel
        {
            public string Id { get; set; }

            public int Qtde { get; set; }
            public string Descricao { get; set; }

            public double PrecoUnit { get; set; }

            public int Proposta_id { get; set; }

            
        }

            //Para atender o filtro do relatório


            public List<PropostaModel> ListagemPropostas(string DataDe, string DataAte)
        {
            return RetornarListagemPropostas(DataDe, DataAte);
        }

        //Listagem Geral
        public List<PropostaModel> ListagemPropostas()
        {
            return RetornarListagemPropostas("1900/01/01", "2200/01/01");
        }


        public List<PropostaModel> ListaPropostas()
        {
            // A lista deve ser do tipo PropostaModel, já que estamos lidando com propostas completas
            List<PropostaModel> lista = new List<PropostaModel>();

            DAL objDAL = new DAL();
            string sql = $"SELECT t1.idpropostas, t1.data, t1.status, t2.nomeclientes AS cliente, t3.nomevendedor AS vendedor,t4.qtde, t4.preco_unit, " +
    "SUM(t4.preco_unit * t4.qtde) AS total_itens " +
    "FROM propostas t1 " +
    "INNER JOIN clientes t2 ON t1.id_clientes = t2.idclientes " +
    "INNER JOIN vendedor t3 ON t1.id_vendedor = t3.idvendedor " +
    "INNER JOIN item t4 ON t1.idpropostas = t4.id_proposta " +
    "GROUP BY t1.idpropostas, t1.data, t1.status, t2.nomeclientes, t3.nomevendedor";
            DataTable dt = objDAL.RetDataTable(sql);

            // Adiciona item por item à lista 
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Cria um objeto PropostaModel
                var proposta = new PropostaModel
                {
                    Id = dt.Rows[i]["idpropostas"].ToString(),
                    Status = dt.Rows[i]["status"].ToString(),
                    Cliente_id = dt.Rows[i]["cliente"].ToString(),
                    Vendedor_id = dt.Rows[i]["vendedor"].ToString(),
                    ValorTotal = double.Parse(dt.Rows[i]["total_itens"].ToString()),
                    // Outros campos
                };

                // Aqui você pode preencher os itens de proposta (se necessário)
                proposta.Itens.Add(new PropostaModel.ItemModel
                {
                 
                    Qtde = int.Parse(dt.Rows[i]["qtde"].ToString()),
                   // Descricao = dt.Rows[i]["descricao"].ToString(),
                    PrecoUnit = double.Parse(dt.Rows[i]["preco_unit"].ToString()),
                  
                   

                });

                // Adiciona a proposta à lista de PropostaModel
                lista.Add(proposta);
            }

            return lista;
        }

        public PropostaModel RetornarProposta(int? id)
        {

            PropostaModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT t1.idpropostas, t1.data, t1.status, t2.nomeclientes AS cliente, t3.nomevendedor AS vendedor,t4.qtde, t4.preco_unit, " +
            "SUM(t4.preco_unit * t4.qtde) AS total_itens " +
            "FROM propostas t1 " +
            "INNER JOIN clientes t2 ON t1.id_clientes = t2.idclientes " +
            "INNER JOIN vendedor t3 ON t1.id_vendedor = t3.idvendedor " +
            "INNER JOIN item t4 ON t1.idpropostas = t4.id_proposta " 
            + $"WHERE t1.idpropostas = '{id}'";
            DataTable dt = objDAL.RetDataTable(sql);



            // Cria um objeto PropostaModel
            item = new PropostaModel
            {
                Id = dt.Rows[0]["idpropostas"].ToString(),
                Status = dt.Rows[0]["status"].ToString(),
                Cliente_id = dt.Rows[0]["cliente"].ToString(),
                Vendedor_id = dt.Rows[0]["vendedor"].ToString(),
                ValorTotal = double.Parse(dt.Rows[0]["total_itens"].ToString()),
                // Outros campos
            };

            // Aqui você pode preencher os itens de proposta (se necessário)
            item.Itens.Add(new PropostaModel.ItemModel
            {

                Qtde = int.Parse(dt.Rows[0]["qtde"].ToString()),
                // Descricao = dt.Rows[i]["descricao"].ToString(),
                PrecoUnit = double.Parse(dt.Rows[0]["preco_unit"].ToString()),



            });


            return item;
        }
    

        public List<PropostaModel> RetornarListagemPropostas(string DataDe, string DataAte)
        {
            List<PropostaModel> lista = new List<PropostaModel>();
            PropostaModel item;
            DAL objDAL = new DAL();
            string sql = $"Select t1.idpropostas, t1.data, t1.status,t1.data_finalizacao, t2.nomeclientes as cliente, " +
            $"t3.nomevendedor as vendedor from propostas t1 inner join clientes t2 on t1.id_clientes = t2.idclientes inner join vendedor t3 on t1.id_vendedor = t3.idvendedor "
            + $"WHERE t1.data_finalizacao >='{DataDe}' and t1.data_finalizacao <= '{DataAte}' order by data_finalizacao, valor";
            DataTable dt = objDAL.RetDataTable(sql);

            for (int i = 0; i < dt.Rows.Count; i++)
            { 

                item = new PropostaModel
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
