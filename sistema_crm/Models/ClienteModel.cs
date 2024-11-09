using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using System.Data;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace sistema_crm.Models
{
    public class ClienteModel
    {
            public string Id { get; set; }

            [Required(ErrorMessage ="Informe o Nome do cliente")]
            public string Nome { get; set; }

            [Required(ErrorMessage = "Informe o CPF do cliente")]
            public string CNPJ { get; set; }
            
            public string Bairro { get; set; }
            public string Cidade { get; set; }
            
            public string CEP { get; set; }

            public string End {  get; set; }

            public string UF { get; set; }

            public string Telefone { get; set; }
            public string Situacao { get; set; }
            
            public string NomeComprador { get; set; }
            public string Email { get; set; }

            public string Segmento { get; set; }



        public List<ClienteModel> ListarClientes()
        {
            List<ClienteModel> lista = new List<ClienteModel>();
            ClienteModel item;
            DAL objDAL = new DAL();
            string sql = "SELECT idclientes, nomeclientes, cnpjclientes, telcliente, situacaocliente FROM Clientes order by nomeclientes asc";
            DataTable dt = objDAL.RetDataTable(sql);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new ClienteModel
                {
                    Id = dt.Rows[i]["idclientes"].ToString(),
                    Nome = dt.Rows[i]["nomeclientes"].ToString(),
                    CNPJ = dt.Rows[i]["cnpjclientes"].ToString(),
                    Telefone = dt.Rows[i]["telcliente"].ToString(),
                    Situacao = dt.Rows[i]["situacaocliente"].ToString()
                };
                lista.Add(item);
            }

            return lista;
        }

        public ClienteModel RetornarCliente(int? id)
        {
            ClienteModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT idclientes, nomeclientes, cnpjclientes, endclientes, bairro, cep, cidade, ufcliente, telcliente, situacaocliente, nomecomprador, email, segmento FROM clientes WHERE idclientes = '{id}' order by nomeclientes desc";
            DataTable dt = objDAL.RetDataTable(sql);



            item = new ClienteModel
            {
                Id = dt.Rows[0]["idclientes"].ToString(),
                Nome = dt.Rows[0]["nomeclientes"].ToString(),
                CNPJ = dt.Rows[0]["cnpjclientes"].ToString(),
                CEP = dt.Rows[0]["cep"].ToString(),
                Cidade = dt.Rows[0]["cidade"].ToString(),
                Bairro = dt.Rows[0]["bairro"].ToString(),
                End = dt.Rows[0]["endclientes"].ToString(),
                UF = dt.Rows[0]["ufcliente"].ToString(),
                Telefone = dt.Rows[0]["telcliente"].ToString(),
                Situacao = dt.Rows[0]["situacaocliente"].ToString(),
                Segmento = dt.Rows[0]["segmento"].ToString(),
                NomeComprador = dt.Rows[0]["nomecomprador"].ToString(), 
                Email = dt.Rows[0]["email"].ToString()
            };

            return item;
        }


        public bool CnpjExiste(string cnpj)
        {
            DAL objDAL = new DAL();
            bool existe = false;

            string sql = $"SELECT COUNT(1) FROM Clientes WHERE cnpjclientes = @cnpj;";

                var parametros = new Dictionary<string, object>
                {
                    { "@cnpj", cnpj }
                };

            // Executa a consulta e obtém o DataTable
            DataTable dt = objDAL.RetornarDataTable(sql, parametros);

            // Verifica se a contagem é maior que zero
            if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                existe = true;
            }


            return existe;
        }

        //INSERT OU UPDATE
        public void Gravar()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;

            if (Id != null)
            {
                sql = $"UPDATE CLIENTES SET nomeclientes = '{Nome}', cnpjclientes = '{CNPJ}' , endclientes = '{End}', cep = '{CEP}', bairro = '{Bairro}', cidade = '{Cidade}', ufcliente = '{UF}', telcliente = '{Telefone}', situacaocliente='{Situacao}', nomecomprador = '{NomeComprador}', email='{Email}', segmento= '{Segmento}' WHERE idclientes = '{Id}'";
            }
            else
            {
                sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, cep, bairro, cidade, ufcliente, telcliente, situacaocliente, nomecomprador, email, segmento) VALUES('{Nome}', '{CNPJ}', '{End}', '{CEP}', '{Bairro}', '{Cidade}', '{UF}', '{Telefone}', '{Situacao}', '{NomeComprador}', '{Email}', '{Segmento}')";
            }

            objDAL.ExecutarComandoSQL(sql);
        }

        public void Insert()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;

           
                sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, ufcliente, telcliente, situacaocliente, nomecomprador, email, segmento) VALUES('{Nome}', '{CNPJ}', '{End}', '{UF}', '{Telefone}', '{Situacao}', '{NomeComprador}', '{Email}', '{Segmento}')";
            

            objDAL.ExecutarComandoSQL(sql);
        }

        //DELETE
        public void Excluir(int id)
        {
            DAL objDAL = new DAL();
            string sql = $"DELETE FROM CLIENTES WHERE idclientes='{id}'";
            objDAL.ExecutarComandoSQL(sql);
        }
    }
}
