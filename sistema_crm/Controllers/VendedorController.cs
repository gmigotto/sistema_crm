using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using sistema_crm.Models;
using Microsoft.EntityFrameworkCore;
using sistema_crm.Uteis;
using static sistema_crm.Models.VendedorModel;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;


namespace sistema_crm.Controllers
{
    public class VendedorController : Controller
    {
        private readonly VendedorModel _context;

        private const double MetaVendas = 10000;

        [HttpGet]
        public IActionResult DownloadVendedoresCsv()
        {
            var vendedorModel = new VendedorModel();
            var vendedores = vendedorModel.ListarVendedores(); // Obtenha a lista de vendedores

            // Gera o CSV
            var csv = new StringBuilder();
            csv.AppendLine("Id,Nome,CPF,Nascimento,Telefone,Endereco,Email,Status,DataADM");

            foreach (var vendedor in vendedores)
            {
                csv.AppendLine($"{vendedor.Id},{vendedor.Nome},{vendedor.CPF},{vendedor.Nascimento},{vendedor.Telefone},{vendedor.Endereco},{vendedor.Email},{vendedor.Status},{vendedor.DataADM}");
            }

            // Retorna o arquivo CSV
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "vendedores.csv");
        }


        public VendedorController()
        {
            _context = new VendedorModel();
        }

        [HttpGet]
        public IActionResult Lista(int? page)
        {
            var vendedor = new VendedorModel().ListarVendedores();
           // ViewBag.ListaVendedores = new VendedorModel().ListarVendedores();
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(vendedor.ToPagedList(pageNumber, pageSize));
        }


        [HttpGet]
        public JsonResult ObterProgressoVendas(VendedorModel vendedor)
        {
             
            double totalVendas = vendedor.ObterVendasDoMes();
            double progresso = MetaVendas > 0 ? (totalVendas / (double)MetaVendas) * 100 : 0;
            return Json(new { progresso = progresso, totalVendas = totalVendas, meta = MetaVendas });
        }
        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id != null)
            {
                // Caregar o registro de cliente em uma viewbag

                ViewBag.Vendedor = new VendedorModel().RetornarVendedor(id);
            }

            return View();
        }

        [HttpGet]
        public IActionResult Criar(int? id)
        {
            if (id != null)
            {
                //Carregar o registro do vendedor numa ViewBag
                ViewBag.Vendedor = new VendedorModel().RetornarVendedor(id);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Criar(VendedorModel vendedor)
        {
            if (!ValidarCPF(vendedor.CPF))
            {
                ModelState.AddModelError("CPF", "CPF inválido");
                return View();


            }
            else
            {
                vendedor.Gravar();
                return RedirectToAction("Lista", "Vendedor");

            }


        }


        public bool ValidarCPF(string CPF)
        {
            CPF = Regex.Replace(CPF, @"[^\d]", "");

            // Verifica se o CPF possui 11 dígitos após a remoção de caracteres não numéricos
            if (CPF.Length != 11)
            {
                return false;
            }

            // Verifica CPFs com todos os dígitos iguais (111.111.111-11, etc.)
            if (IsSameDigits(CPF))
            {
                return false;
            }

            // Calcula e verifica o primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(CPF[i].ToString()) * (10 - i);
            }
            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            // Verifica o primeiro dígito verificador
            if (int.Parse(CPF[9].ToString()) != digit1)
            {
                return false;
            }

            // Calcula e verifica o segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += int.Parse(CPF[i].ToString()) * (11 - i);
            }
            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            // Verifica o segundo dígito verificador
            if (int.Parse(CPF[10].ToString()) != digit2)
            {
                return false;
            }

            return true;
        }

        private static bool IsSameDigits(string CPF)
        {
            for (int i = 1; i < CPF.Length; i++)
            {
                if (CPF[i] != CPF[0])
                {
                    return false;
                }
            }
            return true;
        }
        public IActionResult UpdateController(VendedorModel vendedor)
        {
            vendedor.Update();
            return RedirectToAction("Lista", "Vendedor");
        }

        public IActionResult Excluir(int id)
        {
            ViewData["IdExcluir"] = id;
            return View();
        }

        public IActionResult ExcluirVendedor(int id)
        {
            new VendedorModel().Excluir(id);
            return View();
        }

        // Action que gera 5000 vendedores falsos
        public IActionResult GerarDados()
        {
            // Configuração do Faker para gerar vendedores falsos
            var vendedorFaker = new Faker<VendedorModel>()
                .RuleFor(v => v.Id, f => Guid.NewGuid().ToString())
                .RuleFor(v => v.Nome, f => f.Person.FullName)
                .RuleFor(v => v.CPF, f => f.Random.ReplaceNumbers("###.###.###-##"))
                .RuleFor(v => v.Nascimento, f => f.Date.Past(40, DateTime.Now.AddYears(-18)).ToString("dd/MM/yyyy"))
                .RuleFor(v => v.Telefone, f => f.Phone.PhoneNumber())
                .RuleFor(v => v.Endereco, f => f.Address.FullAddress())
                .RuleFor(v => v.Email, f => f.Internet.Email())
                .RuleFor(v => v.Senha, f => f.Internet.Password())
                .RuleFor(v => v.Status, f => f.PickRandom(new[] { "Ativo", "Inativo" }))
                .RuleFor(v => v.DataADM, f => f.Date.Past(10).ToString("dd/MM/yyyy"));


            // Gera 5000 vendedores falsos
            List<VendedorModel> vendedoresFalsos = vendedorFaker.Generate(10);

            foreach (var vendedor in vendedoresFalsos)
            {
                vendedor.Gravar();
            }


            return Content("5.000 vendedores falsos foram gerados e salvos no banco de dados com sucesso!");
        }


        [HttpPost]
        public IActionResult AtividadeVendedor(int id)
        {

            var vendedor = _context.Vendedores.FirstOrDefault(v => v.Id == id.ToString());


            if (vendedor == null)
            {
                return NotFound();
            }

            // Buscar as atividades do vendedor pelo ID do vendedor
            var atividades = _context.Atividades.Where(a => a.Idvendedor == id.ToString()).ToList();

            // Passar o vendedor e as atividades para a view
            ViewBag.Vendedor = vendedor;
            ViewBag.Atividades = atividades ?? new List<AtividadeModel>();

            return View();
        }

        public IActionResult AtividadeVendedor()
        {


            return View();
        }



    }
}