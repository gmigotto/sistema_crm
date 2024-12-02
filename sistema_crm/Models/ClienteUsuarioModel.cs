using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using System.Data;
using System.ComponentModel.DataAnnotations;
using MercadoPago.Resource.User;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.Http;

namespace sistema_crm.Models
{
    public class ClienteUsuarioModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Informe o Nome do cliente")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Informe o CNPJ do cliente")]
        public string CNPJ { get; set; }

        public string Bairro { get; set; }
        public string Cidade { get; set; }

        public string CEP { get; set; }

        public string End { get; set; }

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
        public List<ClienteUsuarioModel> ListarClientes(HttpContext httpContext, string situacao = "todos") { 
            List<ClienteUsuarioModel> lista = new List<ClienteUsuarioModel>();
            ClienteUsuarioModel item;
            DAL objDAL = new DAL();
            
            string vendedorId = httpContext.Session.GetString("IdUsuarioLogado");

            // Verifica se o ID não está vazio
            if (string.IsNullOrEmpty(vendedorId))
            {
                // Caso o vendedorId esteja vazio ou nulo, retornar uma lista vazia ou outro tratamento necessário
                return lista;
            }

            // Modificar a consulta para filtrar pelos clientes do vendedor logado
            string sql = $"SELECT idclientes, nomeclientes, cnpjclientes, telcliente, situacaocliente " +
                $"FROM Clientes WHERE fkvendedor = '{vendedorId}'";

            // Adiciona o filtro de situação, se necessário
            if (situacao != "todos")
            {
                sql += $" AND situacaocliente = '{situacao}'";
            }

            // Adiciona ordenação
            sql += " ORDER BY nomeclientes ASC";

            DataTable dt = objDAL.RetDataTable(sql);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new ClienteUsuarioModel
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

        public ClienteUsuarioModel RetornarCliente(int? id)
        {
            ClienteUsuarioModel item;
            DAL objDAL = new DAL();
            string sql = $"SELECT idclientes, nomeclientes, cnpjclientes, endclientes, ufcliente, telcliente, situacaocliente FROM clientes WHERE idclientes = '{id}' order by nomeclientes desc";
            DataTable dt = objDAL.RetDataTable(sql);



            item = new ClienteUsuarioModel
            {
                Id = dt.Rows[0]["idclientes"].ToString(),
                Nome = dt.Rows[0]["nomeclientes"].ToString(),
                CNPJ = dt.Rows[0]["cnpjclientes"].ToString(),
                End = dt.Rows[0]["endclientes"].ToString(),
                UF = dt.Rows[0]["ufcliente"].ToString(),
                Telefone = dt.Rows[0]["telcliente"].ToString(),
                Situacao = dt.Rows[0]["situacaocliente"].ToString()
            };

            return item;
        }
            

        //INSERT OU UPDATE
        public void Gravar(HttpContext httpContext)
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string vendedorId = httpContext.Session.GetString("IdUsuarioLogado");

            if (!string.IsNullOrEmpty(vendedorId))
            {
                // Recupera o idadmin associado ao idvendedor
                string queryGetAdminId = $@"
                    SELECT idadmin 
                    FROM vendedor 
                    WHERE idvendedor = '{vendedorId}'";
                DataTable adminResult = objDAL.RetDataTable(queryGetAdminId);

                if (adminResult.Rows.Count > 0)
                {
                    // Obtém o idadmin do resultado da consulta
                    string adminId = adminResult.Rows[0]["idadmin"].ToString();

                    if (Id != null) // Atualização de cliente existente
                    {
                        sql = $@"
                    UPDATE CLIENTES 
                    SET nomeclientes = '{Nome}', 
                        cnpjclientes = '{CNPJ}', 
                        endclientes = '{End}', 
                        cep = '{CEP}', 
                        bairro = '{Bairro}', 
                        cidade = '{Cidade}', 
                        ufcliente = '{UF}', 
                        telcliente = '{Telefone}', 
                        situacaocliente = '{Situacao}', 
                        nomecomprador = '{NomeComprador}', 
                        email = '{Email}', 
                        segmento = '{Segmento}', 
                        fkvendedor = '{vendedorId}', 
                        fkadmin = '{adminId}' 
                    WHERE idclientes = '{Id}'";
                    }
                    else // Inserção de novo cliente
                    {
                        sql = $@"
                    INSERT INTO clientes(
                        nomeclientes, 
                        cnpjclientes, 
                        endclientes, 
                        cep, 
                        bairro, 
                        cidade, 
                        ufcliente, 
                        telcliente, 
                        situacaocliente, 
                        fkvendedor, 
                        fkadmin, 
                        nomecomprador, 
                        email, 
                        segmento
                    ) 
                    VALUES(
                        '{Nome}', 
                        '{CNPJ}', 
                        '{End}', 
                        '{CEP}', 
                        '{Bairro}', 
                        '{Cidade}', 
                        '{UF}', 
                        '{Telefone}', 
                        '{Situacao}', 
                        '{vendedorId}', 
                        '{adminId}', 
                        '{NomeComprador}', 
                        '{Email}', 
                        '{Segmento}'
                    )";
                    }

                    // Executa o comando SQL
                    objDAL.ExecutarComandoSQL(sql);
                }
                else
                {
                    throw new Exception("Erro: Não foi possível encontrar o administrador associado ao vendedor logado.");
                }
            }
            else
            {
                throw new Exception("Erro: O ID do vendedor logado não está disponível na sessão.");
            }
        }

        public void Insert(HttpContext httpContext)
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;
            string vendedorId = httpContext.Session.GetString("IdUsuarioLogado");


            sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, ufcliente, telcliente, situacaocliente, fkvendedor) VALUES('{Nome}', '{CNPJ}', '{End}', '{UF}', '{Telefone}', '{Situacao}', '{vendedorId}')";
            

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
      

    }
}
