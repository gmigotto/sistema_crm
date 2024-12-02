using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using static sistema_crm.Models.VendedorModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using MySql.Data.MySqlClient;
using Grpc.Core;
using static iTextSharp.text.pdf.AcroFields;
using System.Text.RegularExpressions;

namespace sistema_crm.Models
{
    public class VendedorUsuarioModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Informe nome do vendedor")]
        public string Nome { get; set; }


        public string CPF { get; set; }

        public string Nascimento { get; set; }


        public string Telefone { get; set; }

        public string Endereco { get; set; }

        [Required(ErrorMessage = "Informe o e-mail do vendedor")]

        public string Email { get; set; }
        public string Senha { get; set; }

        public string Status { get; set; }

        public string DataADM { get; set; }

        public double ValorVendas { get; set; }



        public virtual ICollection<AtividadeModel> Atividades { get; set; }
        



            public List<ClienteModel> RetornarListaClientes()
            {
                return new ClienteModel().ListarClientes();
            }

            public List<VendedorModel> RetornarListaVendedores()
            {
                return new VendedorModel().ListarVendedores();
            }



        
    }
}


