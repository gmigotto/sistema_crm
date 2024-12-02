using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace sistema_crm.Models
{
    public class HomeModel
    {
        public string Id { get; set; }


        public string Nome { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }
        public string Empresa { get; set; }
        public string CNPJ { get; set; }

        public double Negociacao { get; set; }

        public string Mes {get; set; }
        public int TotalProposta { get; set; }
        public int TotalPropostaVendas { get; set; }

        public double Conversao { get; set; }


        public void GravarGestor()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;

            sql = $"INSERT INTO Gestores(Nome, Email, Senha, Empresa, CNPJ) VALUES('{Nome}', '{Email}', '{Senha}', '{Empresa}', '{CNPJ}')";

            objDAL.ExecutarComandoSQL(sql);

        }

        public HomeModel RetornarNegociacao()
        {
            HomeModel item;
            DAL objDAL = new DAL();
            string sql = @"SELECT SUM(t1.qtde * t1.preco_unit) AS total_negociacao FROM item t1 JOIN propostas t2 on t2.idpropostas = t1.id_proposta WHERE t2.status='EM NEGOCIAÇÃO';";

            DataTable dt = objDAL.RetDataTable(sql);


            item = new HomeModel
            {

                Negociacao = double.Parse(dt.Rows[0]["total_negociacao"].ToString())

            };

            return item;
        }


        public HomeModel TaxaConversaoPropostas()
        {
            HomeModel item;
            DAL objDAL = new DAL();
            string sql = @"SELECT MONTHNAME(data) AS mes, 
                            COUNT(*) AS total_propostas_realizadas,
                            (SELECT COUNT(*) 
                             FROM propostas 
                             WHERE status = 'VENDA REALIZADA' 
                             AND EXTRACT(YEAR FROM data_finalizacao) = YEAR(CURDATE()) 
                             AND EXTRACT(MONTH FROM data_finalizacao) = MONTH(CURDATE())
                            ) AS total_propostas_venda_no_mes,
                             CASE 
                                WHEN (SELECT COUNT(*) 
                                      FROM propostas 
                                      WHERE status = 'VENDA REALIZADA' 
                                      AND EXTRACT(YEAR FROM data_finalizacao) = YEAR(CURDATE()) 
                                      AND EXTRACT(MONTH FROM data_finalizacao) = MONTH(CURDATE())
                                     ) = 0 THEN 0
                                ELSE ROUND(
                                      (COUNT(*) * 100.0 / 
                                      (SELECT COUNT(*) 
                                       FROM propostas 
                                       WHERE status = 'VENDA REALIZADA' 
                                       AND EXTRACT(YEAR FROM data_finalizacao) = YEAR(CURDATE()) 
                                       AND EXTRACT(MONTH FROM data_finalizacao) = MONTH(CURDATE()))
                                      ), 2)
                            END AS taxa_conversao
                        FROM propostas WHERE
                         EXTRACT(YEAR FROM data) = YEAR(CURDATE()) 
                        AND EXTRACT(MONTH FROM data) = MONTH(CURDATE());";

            DataTable dt = objDAL.RetDataTable(sql);


            item = new HomeModel
            {
                Mes = dt.Rows[0]["mes"].ToString(),
                TotalProposta = int.Parse(dt.Rows[0]["total_propostas_realizadas"].ToString()),
                TotalPropostaVendas = int.Parse(dt.Rows[0]["total_propostas_venda_no_mes"].ToString()),
                Conversao = double.Parse(dt.Rows[0]["taxa_conversao"].ToString())

            };

            return item;
        }



    }

}
    

