using Bogus.DataSets;
using sistema_crm.Uteis;
using System.ComponentModel.DataAnnotations;

namespace sistema_crm.Models
{
    public class ItemModel
    {
        public string Id { get; set; }

        public int Qtde { get; set; }
        public string Descricao { get; set; }

        public double PrecoUnit {  get; set; }

        public int Proposta_id { get; set; }


        public virtual PropostaModel Proposta { get; set; }

        public void GravarItem () 
        {

            DAL objDAL = new DAL();
            string sql = string.Empty;

            if (Id != null)
            {
                sql = $"UPDATE ITEM SET qtde = '{Qtde}', descricao = '{Descricao}' , preco_unit = '{PrecoUnit}', id_proposta = '{Proposta_id}' WHERE id_item = '{Id}'";
            }
            else
            {
                sql = $"INSERT INTO ITEM(qtde, descricao, preco_unit, id_proposta) VALEUS('{Qtde}', '{Descricao}', ''{PrecoUnit.ToString().Replace(",", ".")}', '{Proposta_id}')";
            }

            objDAL.ExecutarComandoSQL(sql);
        }



       

    }
}




