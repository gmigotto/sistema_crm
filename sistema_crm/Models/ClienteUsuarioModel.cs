﻿using System;
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

            [Required(ErrorMessage ="Informe o Nome do cliente")]
            public string Nome { get; set; }

            [Required(ErrorMessage = "Informe o CPF do cliente")]
            public string CNPJ { get; set; }

            public string End {  get; set; }

            public string UF { get; set; }

            public string Telefone { get; set; }
            public string Situacao { get; set; }

            public int VendedorId { get; set; }
        public List<ClienteUsuarioModel> ListarClientes(HttpContext httpContext) { 
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
            string sql = $"SELECT idclientes, nomeclientes, cnpjclientes, telcliente, situacaocliente FROM Clientes WHERE fkvendedor= '{vendedorId}' ORDER BY nomeclientes ASC";

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

            // Verifica se o ID não está vazio
          
            if (Id != null)
            {
                sql = $"UPDATE CLIENTES SET nomeclientes = '{Nome}', cnpjclientes = '{CNPJ}' , endclientes = '{End}', ufcliente = '{UF}', telcliente = '{Telefone}', situacaocliente='{Situacao}' WHERE idclientes = '{Id}'";
            }
            else
            {
                sql = $"INSERT INTO clientes(nomeclientes, cnpjclientes, endclientes, ufcliente, telcliente, situacaocliente, fkvendedor) VALUES('{Nome}', '{CNPJ}', '{End}', '{UF}', '{Telefone}', '{Situacao}', '{vendedorId}')";
            }

            objDAL.ExecutarComandoSQL(sql);
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
    }
}
