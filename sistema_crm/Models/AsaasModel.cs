using Bogus.DataSets;
using sistema_crm.Uteis;
using System.Data;

namespace sistema_crm.Models
{
    public class AssasModel
    {
        public int Id { get; set; }

        public string? Cpf { get; set; }

        public string? Cnpj { get; set; }

        public string? Asaas { get; set; } = string.Empty;

        public void GravarCustomerAsaas()
        {
            DAL objDAL = new DAL();
            string sql = string.Empty;

            sql = $"INSERT INTO asaas(cpf, cnpj, asaas) VALUES('{Cpf}', '{Cnpj}', '{Asaas}')";

            objDAL.ExecutarComandoSQL(sql);

        }

        public AssasModel? VerificaSeExisteAsaasCustomer(string cpf)
        {
            AssasModel? asaas_data = null;
            DAL objDAL = new DAL();
            string sql = $"SELECT * FROM asaas WHERE cpf = {cpf}";
            DataTable dt = objDAL.RetDataTable(sql);

            if (dt.Rows.Count > 0) // Verifica se há registros
            {
                asaas_data = new AssasModel
                {
                    Id = Convert.ToInt32(dt.Rows[0]["id"]),
                    Cpf = dt.Rows[0]["cpf"].ToString(),
                    Cnpj = dt.Rows[0]["cnpj"].ToString(),
                    Asaas = dt.Rows[0]["asaas"].ToString()
                };
            }

            return asaas_data;
        }
    }
}