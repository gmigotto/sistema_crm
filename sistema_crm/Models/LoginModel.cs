using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using sistema_crm.Uteis;
using System.ComponentModel.DataAnnotations;
using MySql.Data.MySqlClient;

namespace sistema_crm.Models
{
    public class LoginModel
    {
        public string Id { get; set; }

        public string Nome { get; set; }

        [Required(ErrorMessage="Informe o e-mail do usuário!")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage="O e-mail informado é inválido!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Informe a senha do usuário!")]
        public string Senha { get; set; }

        //Existe um problema grave de segurança com essa abordagem => SQL Injection
        //Vamos depois criar um método mais adequado
        public bool ValidarLogin()
        {
            string sql = $"SELECT idvendedor, nomevendedor FROM VENDEDOR WHERE EMAIL=@email AND SENHA=@senha";
            MySqlCommand Command = new MySqlCommand();
            Command.CommandText = sql;
            Command.Parameters.AddWithValue("@email", Email);
            Command.Parameters.AddWithValue("@senha", Senha);

            DAL objDAL = new DAL();

            DataTable dt = objDAL.RetDataTable(Command);
            if (dt.Rows.Count ==1)
            {
                Id = dt.Rows[0]["idvendedor"].ToString();
                Nome = dt.Rows[0]["nomevendedor"].ToString();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ValidarLoginAdm()
        {
            string sql = $"SELECT idGestores, nome FROM Gestores WHERE Email='{Email}' AND Senha='{Senha}'";

            DAL objDAL = new DAL();

            DataTable dt = objDAL.RetDataTable(sql);
            if (dt.Rows.Count == 1)
            {
                Id = dt.Rows[0]["idGestores"].ToString();
                Nome = dt.Rows[0]["nome"].ToString();




                return true;
            }
            else
            {
                return false;
            }


        }
    }
}
