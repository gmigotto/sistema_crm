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
    public class VendedorUsuarioModel
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
        public class AtividadeUsuarioModel
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


       
        public List<AtividadeModel> ListarAtividades(int id)
        {
            List<AtividadeModel> lista = new List<AtividadeModel>();

            AtividadeModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT * FROM Atividade WHERE idclientes = '{id}' order by dtcontato desc";
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

        public void CriarAtividade(AtividadeModel atividade, int id)
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string data = DateTime.Now.Date.ToString("yyyy/MM/dd");

            sql = $"INSERT INTO Atividade (contato, dtcontato, observacao, idclientes, idvendedor ) VALUES('{atividade.Tipo_contato}', '{atividade.DT_contato}', '{atividade.Obs}', '{id}', '{atividade.Idvendedor}')";



            objDAL.ExecutarComandoSQL(sql);

        }

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


