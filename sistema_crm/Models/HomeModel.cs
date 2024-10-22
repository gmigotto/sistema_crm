using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using sistema_crm.Uteis;
using Microsoft.AspNetCore.Mvc;

namespace sistema_crm.Models
{
    public class HomeModel
    {
        public string Id { get; set; }


        public string Nome { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }
        public void GravarGestor()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;

            sql = $"INSERT INTO Gestores(Nome, Email, Senha) VALUES('{Nome}', '{Email}', '{Senha}')";

            objDAL.ExecutarComandoSQL(sql);

        }

       



    }

}
    

