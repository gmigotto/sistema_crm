using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using sistema_crm.Models;

namespace sistema_crm.Controllers
{
    public class RelatorioController : Controller
    {
        public IActionResult Index()
        {
           
            return View();
        }

      


        public IActionResult GraficoVendas()
        {
            
            List<RelatorioModel> lista = new RelatorioModel().RetornarGrafico();
            ViewBag.RetornarGrafico = lista;

            List<RelatorioModel> lista_ = new RelatorioModel().RetornarGraficoClientes();
            ViewBag.RetornarGraficoClientes = lista_;

            ViewBag.Relatorio = new RelatorioModel().RetornarNegociacao(); 

            string valores = "";
            string labels = "";
            string cores = "";

            var random = new Random();
            // Percorre a lista de itens para compor o gráfico de barras
            for (int i = 0; i < lista.Count; i++)
            {
                valores += lista[i].Valor.ToString() + ",";
                labels += "'" + lista[i].Mes.ToString() + "',";
                // escolher aleatoriamente as cores para compor as barras
                cores += "'" + String.Format("#{0:X6}", random.Next(0x1000000)) + "',";
            }

            ViewBag.Valores = valores.TrimEnd(',');
            ViewBag.Labels = labels.TrimEnd(',');
            ViewBag.Cores = cores.TrimEnd(',');


            string valores2 = "";
            string labels2 = "";
            string cores2 = "";

            var random1 = new Random();
            // Percorre a lista de itens para compor o gráfico de barras
            for (int j = 0; j < lista_.Count; j++)
            {
                valores2 += lista_[j].ValorClientes.ToString() + ",";
                labels2 += "'" + lista_[j].Clientes.ToString() + "',";
                // escolher aleatoriamente as cores para compor as barras
                cores2 += "'" + String.Format("#{0:X6}", random1.Next(0x1000000)) + "',";
            }

            ViewBag.Valores2 = valores2.TrimEnd(',');
            ViewBag.Labels2 = labels2.TrimEnd(',');
            ViewBag.Cores2 = cores2.TrimEnd(',');


            return View();
        }



        [HttpGet]
        public IActionResult Vendas()
        {
            return View();
        }



        [HttpPost]
        /*   public IActionResult Vendas(RelatorioModel relatorio)
           {
               if (relatorio.DataDe.Year == 1)
               {
                   ViewBag.ListaVendas = new VendaModel().ListagemVendas();
               }
               else
               {
                   string DataDe = relatorio.DataDe.ToString("yyyy/MM/dd");
                   string DataAte = relatorio.DataAte.ToString("yyyy/MM/dd");
                   ViewBag.ListaVendas = new VendaModel().ListagemVendas(DataDe, DataAte);
               }
               return View();
           }*/


      

        public IActionResult Comissao()
        {
            return View();
        }

    }
}