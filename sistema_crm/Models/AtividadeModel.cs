using sistema_crm.Uteis;
using static sistema_crm.Models.VendedorModel;
using System.Data;

namespace sistema_crm.Models
{
    public class AtividadeUsuarioModel
    {
        public int Id { get; set; }

        public string Obs { get; set; }

        public string Idcliente { get; set; }

        public int Idvendedor { get; set; }

        public string Tipo_contato { get; set; }

        public string DT_contato { get; set; }

        public string Vendedor { get; set; }


        public List<AtividadeModel> ListarAtividades(int id)
        {
            List<AtividadeModel> lista = new List<AtividadeModel>();

            AtividadeModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT * FROM Atividade WHERE idvendedor= '{id}' order by dtcontato desc";
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
                    Idvendedor = Convert.ToInt32(dt.Rows[0]["idvendedor"]),

                };
                lista.Add(item);
            }
            return lista;
        }

        //public void Criar(int id)
        //{
        //    string userId = HttpContext.Session.GetString("IdUsuarioLogado");
        //    DAL objDAL = new DAL();
        //    string sql = string.Empty;
        //    string data = DateTime.Now.Date.ToString("yyyy/MM/dd");

        //    sql = $"INSERT INTO Atividade (contato, dtcontato, observacao, idclientes, idvendedor ) VALUES('{Tipo_contato}', '{data}', '{Obs}', '{id}', '{id}')";



        //    objDAL.ExecutarComandoSQL(sql);

        //}



        public AtividadeModel RetornarAtividades(int id)
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
