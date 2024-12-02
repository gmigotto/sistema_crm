using sistema_crm.Models;
using sistema_crm.Uteis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Grpc.Core;
using System.Drawing;
using System.Text.RegularExpressions;
using DinkToPdf;
using Org.BouncyCastle.Tls.Crypto;

namespace sistema_crm.Models
{
   

    public class RelatorioModel
    {
        public double Valor { get; set; }
        public int CodigoVenda { get; set; }
        public string Mes { get; set; }
        public double Negociacao { get; set; }

        public double ValorClientes { get; set; }
        public string Clientes { get; set; }

        public string Status { get; set; }


        public List<RelatorioModel> RetornarGrafico()
        {
            DAL objDAL = new DAL();
            string sql = @"
               SELECT 
                    MONTHNAME(t1.data_finalizacao) AS mes, 
                    SUM(t2.qtde * t2.preco_unit) AS total_vendas 
                FROM 
                    propostas t1 
                INNER JOIN 
                    item t2 ON t1.idpropostas = t2.id_proposta
                WHERE 
                    t1.status = 'VENDA REALIZADA' 
                    AND EXTRACT(YEAR FROM t1.data_finalizacao) = 2024 
                GROUP BY 
                    MONTH(t1.data_finalizacao), MONTHNAME(t1.data_finalizacao)
                ORDER BY 
                    MONTH(t1.data_finalizacao);";
            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                item.Mes = (dt.Rows[i]["mes"].ToString()); 
                lista.Add(item);        
            }
            return lista;
        }

        public List<RelatorioModel> RetornarGraficoClientes()
        {
            DAL objDAL = new DAL();
            string sql = @"SELECT t3.nomeclientes, SUM(t1.qtde * t1.preco_unit) AS total_vendas FROM  item t1 JOIN propostas t2 on  t2.idpropostas = t1.id_proposta JOIN clientes t3 ON t2.id_clientes = t3.idclientes WHERE t2.status= 'VENDA REALIZADA' GROUP BY t3.nomeclientes;";
            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.ValorClientes = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                item.Clientes = (dt.Rows[i]["nomeclientes"].ToString());
                lista.Add(item);
            }
            return lista;
        }

        public List<RelatorioModel> RetornarGraficoVendasUltimosMeses()
        {
            DAL objDAL = new DAL();
            string sql = @"SELECT  MONTHNAME(t1.data_finalizacao) AS mes, SUM(t2.qtde * t2.preco_unit) AS total_vendas FROM propostas t1 INNER JOIN 
                item t2 ON t1.idpropostas = t2.id_proposta WHERE  t1.status = 'VENDA REALIZADA'   AND t1.data_finalizacao >= DATE_SUB(CURDATE(), INTERVAL 3 MONTH) GROUP BY 
                MONTH(t1.data_finalizacao), MONTHNAME(t1.data_finalizacao) ORDER BY  MONTH(t1.data_finalizacao);";
            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                item.Mes = (dt.Rows[i]["mes"].ToString());
                lista.Add(item);
            }
            return lista;
        }

        public List<RelatorioModel> RetornarGraficoValoresStatus()
        {
            DAL objDAL = new DAL();
            string sql = @"SELECT  t1.status AS status, MONTHNAME(t1.data_finalizacao) AS mes, SUM(t2.qtde * t2.preco_unit) AS total_vendas
            FROM  propostas t1 INNER JOIN item t2 ON t1.idpropostas = t2.id_proposta
            WHERE MONTH(t1.data_finalizacao) = MONTH(CURDATE()) AND YEAR(t1.data_finalizacao) = YEAR(CURDATE())
            GROUP BY  t1.status, MONTH(t1.data_finalizacao), MONTHNAME(t1.data_finalizacao)
            ORDER BY MONTH(t1.data_finalizacao), t1.status;";
            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Status = (dt.Rows[i]["status"].ToString());
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                lista.Add(item);
            }
            return lista;
        }


        public List<RelatorioModel> GraficoSegmento()
        {
            DAL objDAL = new DAL();
            string sql = $" SELECT  c.segmento AS Segmento,  COUNT(*) AS QuantidadeClientes FROM clientes c GROUP BY c.segmentoORDER BY QuantidadeClientes DESC;";

            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Status = (dt.Rows[i]["status"].ToString());
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                lista.Add(item);
            }
            return lista;
        }

        public List<RelatorioModel> RetornarGraficoValoresVendedor(int id)
        {
            DAL objDAL = new DAL();
             string sql = @"WITH ultimos_meses AS (
                SELECT MONTHNAME(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)) AS mes, MONTH(DATE_SUB(CURDATE(), INTERVAL 2 MONTH)) AS mes_num
                UNION
                SELECT MONTHNAME(DATE_SUB(CURDATE(), INTERVAL 1 MONTH)), MONTH(DATE_SUB(CURDATE(), INTERVAL 1 MONTH))
                UNION
                SELECT MONTHNAME(CURDATE()), MONTH(CURDATE()))
            SELECT um.mes AS mes, COALESCE(SUM(t2.qtde * t2.preco_unit), 0.00) AS total_vendas
            FROM ultimos_meses um
            LEFT JOIN propostas t1 ON MONTH(t1.data_finalizacao) = um.mes_num
                                    AND t1.status = 'VENDA REALIZADA'
                                    AND t1.data_finalizacao >= DATE_SUB(CURDATE(), INTERVAL 3 MONTH)
                                    AND t1.id_vendedor = @id
            LEFT JOIN item t2 ON t1.idpropostas = t2.id_proposta  GROUP BY um.mes, um.mes_num ORDER BY um.mes_num; ";
            // Parâmetros SQL para o total de vendas
            var parametrosVendas = new Dictionary<string, object>
            {
                { "@id", id }
            };

            // Executa a query para buscar o total de vendas do vendedor
            DataTable dt = objDAL.RetornarDataTable(sql, parametrosVendas);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Mes = (dt.Rows[i]["mes"].ToString());
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                lista.Add(item);
            }
            return lista;
        }

        public List<RelatorioModel> RetornarGraficoPerdidoUltimosMeses()
        {
            DAL objDAL = new DAL();
            string sql = @"SELECT  MONTHNAME(t1.data_finalizacao) AS mes, SUM(t2.qtde * t2.preco_unit) AS total_vendas FROM propostas t1 INNER JOIN 
                item t2 ON t1.idpropostas = t2.id_proposta WHERE  t1.status = 'VENDA REALIZADA'   AND t1.data_finalizacao >= DATE_SUB(CURDATE(), INTERVAL 3 MONTH) GROUP BY 
                MONTH(t1.data_finalizacao), MONTHNAME(t1.data_finalizacao) ORDER BY  MONTH(t1.data_finalizacao);";
            DataTable dt = objDAL.RetDataTable(sql);

            List<RelatorioModel> lista = new List<RelatorioModel>();
            RelatorioModel item;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                item = new RelatorioModel();
                item.Valor = double.Parse(dt.Rows[i]["total_vendas"].ToString());
                item.Mes = (dt.Rows[i]["mes"].ToString());
                lista.Add(item);
            }
            return lista;
        }
        public RelatorioModel RetornarNegociacao()
        {
            RelatorioModel item;
            DAL objDAL = new DAL();
            string sql = @"SELECT SUM(t1.qtde * t1.preco_unit) AS total_negociacao FROM item t1 JOIN propostas t2 WHERE t2.status='EM NEGOCIAÇÃO';";
            DataTable dt = objDAL.RetDataTable(sql);


            item = new RelatorioModel
            {

                Negociacao = double.Parse(dt.Rows[0]["total_negociacao"].ToString())

            };
            
            return item;
        }

       
    }
}



