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

            [Required(ErrorMessage = "Informe o CNPJ do cliente")]
            public string CNPJ { get; set; }
            
            public string Bairro { get; set; }
            public string Cidade { get; set; }
            
            public string CEP { get; set; }

            public string End {  get; set; }

            public string UF { get; set; }

            public string Telefone { get; set; }
            public string Situacao { get; set; }
       
            [Required(ErrorMessage = "Informe o Nome Comprador")]

            public string NomeComprador { get; set; }
            [Required(ErrorMessage = "Informe o Nome Comprador")]
    
            public string Email { get; set; }

            public string Segmento { get; set; }

            public string Vendedor_id { get; set; }

            public string Mes { get; set; }
            public double Valor { get; set; }

        public List<ClienteModel> ListarClientes(string situacao = "todos")
        {
            List<ClienteModel> lista = new List<ClienteModel>();
            ClienteModel item;
            DAL objDAL = new DAL();

            // Base SQL
            string sql = "SELECT idclientes, nomeclientes, cnpjclientes, telcliente, situacaocliente FROM Clientes WHERE 1=1";

            // Adiciona o filtro de situação, se necessário
            if (situacao != "todos")
            {
                sql += $" AND situacaocliente = '{situacao}'";
            }

            sql += " ORDER BY nomeclientes ASC";

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

        public ClienteModel RetornarMediaCliente(int id)
        {
            ClienteModel item;
            DAL objDAL = new DAL();
            string sql = @"WITH Meses AS (
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
            DataTable dt = objDAL.RetDataTable(sql);


            item = new ClienteModel
            {

                Valor = double.Parse(dt.Rows[0]["valor_medio_vendas"].ToString())

            };

            return item;
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
                sql = $"UPDATE CLIENTES SET nomeclientes = '{Nome}', cnpjclientes = '{CNPJ}' , endclientes = '{End}', cep = '{CEP}', bairro = '{Bairro}', cidade = '{Cidade}', ufcliente = '{UF}', telcliente = '{Telefone}', situacaocliente='{Situacao}', nomecomprador = '{NomeComprador}', email='{Email}', segmento= '{Segmento}', fkvendedor= '{Vendedor_id}' WHERE idclientes = '{Id}'";
            }
            else
            {
                sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, cep, bairro, cidade, ufcliente, telcliente, situacaocliente, fkvendedor, nomecomprador, email, segmento) VALUES('{Nome}', '{CNPJ}', '{End}', '{CEP}', '{Bairro}', '{Cidade}', '{UF}', '{Telefone}', '{Situacao}', '{Vendedor_id}','{NomeComprador}', '{Email}', '{Segmento}')";
            }

            objDAL.ExecutarComandoSQL(sql);
        }

        public void Insert()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;


            sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, cep, bairro, cidade, ufcliente, telcliente, situacaocliente, fkvendedor, nomecomprador, email, segmento) VALUES('{Nome}', '{CNPJ}', '{End}', '{CEP}', '{Bairro}', '{Cidade}', '{UF}', '{Telefone}', '{Situacao}', '{Vendedor_id}','{NomeComprador}', '{Email}', '{Segmento}')";


            objDAL.ExecutarComandoSQL(sql);
        }

        //DELETE
        public void Excluir(int id)
        {
            DAL objDAL = new DAL();
            string sql = $"DELETE FROM CLIENTES WHERE idclientes='{id}'";
            objDAL.ExecutarComandoSQL(sql);
        }

        public List<VendedorModel> RetornarListaVendedores()
        {
            return new VendedorModel().ListarVendedores();
        }


        public List<ClienteModel> RetornarGraficoValoresClientes(int id)
        {
            DAL objDAL = new DAL();
            string sql = @"WITH Meses AS (
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
            )
            SELECT 
                t3.nomeclientes,
                m.mes AS mes,
                COALESCE(SUM(vendas.qtde * vendas.preco_unit), 0) AS total_vendas
            FROM 
                Meses m
            CROSS JOIN 
                (SELECT nomeclientes FROM clientes WHERE idclientes = @id) t3
            LEFT JOIN (
                SELECT 
                    MONTH(t2.data_finalizacao) AS mes_num,
                    t1.qtde,
                    t1.preco_unit
                FROM 
                    item t1
                JOIN 
                    propostas t2 ON t2.idpropostas = t1.id_proposta
                WHERE 
                    t2.status = 'VENDA REALIZADA'
                    AND t2.id_clientes = @id
            ) vendas ON m.mes_num = vendas.mes_num
            GROUP BY 
                t3.nomeclientes, m.mes, m.mes_num
            ORDER BY 
                m.mes_num;";

            // Parâmetros SQL para o total de vendas
            var parametrosVendas = new Dictionary<string, object>
            {
                { "@id", id }
            };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dt = objDAL.RetornarDataTable(sql, parametrosVendas);

            List<ClienteModel> lista = new List<ClienteModel>();
            ClienteModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new ClienteModel();
                item.Mes = (dt.Rows[i]["mes"].ToString());
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                lista.Add(item);
            }
            return lista;
        }

    }
}
