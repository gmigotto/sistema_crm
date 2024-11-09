using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using sistema_crm.Models;
using sistema_crm.Uteis;
using Grpc.Core;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Text;
using Bogus.DataSets;
using static iTextSharp.text.pdf.AcroFields;
using MySqlX.XDevAPI;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;



namespace sistema_crm.Controllers
{
    public class RelatorioController : Controller
    {

        private readonly IConverter _converter;

        public RelatorioController(IConverter converter)
        {
            _converter = converter;
        }
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

        public IActionResult ValorNegociacao()
        {
            ViewBag.Relatorio = new RelatorioModel().RetornarNegociacao();
            return View("Menu", "Home");
        }


        [HttpGet]
        public IActionResult Vendas()
        {
            return View();
        }

        private string GerarGraficoVendas(DataTable dtVendas, string titulo)
        {
            int largura = 600;
            int altura = 400;
            var bitmap = new Bitmap(largura, altura);
            var graphics = Graphics.FromImage(bitmap);

            // Fundo branco
            graphics.Clear(Color.White);

            // Configurações de fonte e cor
            Font fontTitulo = new Font("Arial", 16, FontStyle.Bold);
            Font font = new Font("Arial", 10);
            Brush brush = new SolidBrush(Color.Black);

            // Título
            graphics.DrawString(titulo, fontTitulo, brush, new PointF(10, 10));

            // Desenho do gráfico de barras
            int x = 50;
            int y = 100;
            int larguraBarra = 40;
            int espacoEntreBarras = 20;

            int maxValor = dtVendas.AsEnumerable().Max(row => Convert.ToInt32(row["total_vendas"]));

            foreach (DataRow row in dtVendas.Rows)
            {
                string mes = row["mes"].ToString();
                int totalVendas = Convert.ToInt32(row["total_vendas"]);

                // Altura da barra proporcional ao valor máximo
                int alturaBarra = (int)((double)totalVendas / maxValor * 200);

                // Desenha a barra
                graphics.FillRectangle(Brushes.Blue, x, y - alturaBarra, larguraBarra, alturaBarra);

                // Rótulos
                graphics.DrawString(mes, font, brush, x, y + 5);
                graphics.DrawString(totalVendas.ToString("C"), font, brush, x, y - alturaBarra - 20);

                x += larguraBarra + espacoEntreBarras;
            }

            // Salva a imagem temporariamente
            string caminhoImagem = Path.Combine(Path.GetTempPath(), $"grafico_vendas_{Guid.NewGuid()}.png");
            bitmap.Save(caminhoImagem, ImageFormat.Png);

            bitmap.Dispose();
            graphics.Dispose();

            return caminhoImagem;
        }
        public ActionResult GerarRelatoriosPDF(int filtroPeriodo)
        {
            // Chama a função para gerar o PDF com base no filtroPeriodo (exemplo: 3 meses)
            var pdfResult = GerarPDFRelatorio("Relatório de Vendas - Período Personalizado", filtroPeriodo);
            return pdfResult;
        }

        private FileContentResult GerarPDFRelatorio(string titulo, int filtroPeriodo)
        {
            DAL objDAL = new DAL();

            DateTime dataInicio = DateTime.Now.AddMonths(-filtroPeriodo);
            DateTime dataFim = DateTime.Now;

            string sqlProposta = $@"
            SELECT MONTHNAME(t1.data_finalizacao) AS mes, 
                   SUM(t2.qtde * t2.preco_unit) AS total_vendas 
            FROM propostas t1 
            INNER JOIN item t2 ON t1.idpropostas = t2.id_proposta 
            WHERE t1.status = 'VENDA REALIZADA' 
              AND t1.data_finalizacao BETWEEN '{dataInicio:yyyy-MM-dd}' AND '{dataFim:yyyy-MM-dd}'
            GROUP BY MONTH(t1.data_finalizacao), MONTHNAME(t1.data_finalizacao) 
            ORDER BY MONTH(t1.data_finalizacao);";

            DataTable dtVendas = objDAL.RetDataTable(sqlProposta);

            string caminhoImagem = GerarGraficoVendas(dtVendas, "Vendas por Mês");
            // Gerando o conteúdo HTML com os dados do relatório
            var sbHtml = new StringBuilder();
            sbHtml.Append($@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        h1 {{ color: #2c3e50; }}
                        table {{ width: 100%; border-collapse: collapse; }}
                        th, td {{ padding: 8px; text-align: left; border: 1px solid #ddd; }}
                        th {{ background-color: #f2f2f2; }}
                    </style>
                </head>
                <body>
                    <h1>{titulo}</h1>
                    <p>Período: {dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}</p>
                    <table>
                        <tr>
                            <th>Mês</th>
                            <th>Total de Vendas (R$)</th>
                        </tr>");

            foreach (DataRow row in dtVendas.Rows)
            {
                sbHtml.Append("<tr>");
                sbHtml.Append($"<td>{row["mes"]}</td>");
                sbHtml.Append($"<td>{Convert.ToDecimal(row["total_vendas"]):C}</td>");
                sbHtml.Append("</tr>");
            }

            sbHtml.Append(@"
                    </table>
                </body>
            </html>");

            var pdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                }
            };

            // Adicione o conteúdo HTML como um ObjectSettings diretamente à lista `Objects`.
            pdfDocument.Objects.Add(new ObjectSettings
            {
                HtmlContent = sbHtml.ToString(),
                WebSettings = { DefaultEncoding = "utf-8" }
            });

            var file = _converter.Convert(pdfDocument);

            // Remove o arquivo temporário
            if (System.IO.File.Exists(caminhoImagem))
                System.IO.File.Delete(caminhoImagem);

            return File(file, "application/pdf", $"{titulo}.pdf");
        }
    }
}
