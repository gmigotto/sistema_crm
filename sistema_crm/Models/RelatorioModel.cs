using sistema_crm.Models;
using sistema_crm.Uteis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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


        public List<RelatorioModel> RetornarGrafico()
        {
            DAL objDAL = new DAL();
            string sql = @"
            SELECT MONTHNAME(t1.data_finalizacao) AS mes, 
            SUM(t2.qtde * t2.preco_unit) AS total_vendas 
            FROM propostas t1 INNER JOIN item t2
            WHERE t1.status = 'VENDA REALIZADA' 
            AND EXTRACT(YEAR FROM t1.data_finalizacao) = 2024 
            GROUP BY EXTRACT(MONTH FROM t1.data_finalizacao) 
            ORDER BY mes;";
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
            string sql = @"SELECT t3.nomeclientes, SUM(t1.qtde * t1.preco_unit) AS total_vendas FROM  item t1 JOIN propostas t2 JOIN clientes t3 ON t2.id_clientes = t3.idclientes GROUP BY t3.nomeclientes;";
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



