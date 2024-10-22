using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using sistema_crm.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace sistema_crm.Uteis
{
    //Data Access Layer
    public class DAL
    {
        
        private readonly MySqlConnection Connection;

        public DAL()
        {
            string ConnectionString = "Server='localhost';Database='sistema_crm';user='root';password='';Sslmode=none;Charset=utf8;convert zero datetime=True";

            Connection = new MySqlConnection(ConnectionString);
            Connection.Open();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySQL("server=localhost;port=3306;database=nome_do_banco;user=root;password=sua_senha;");

        //}

        //Espera um parâmetro do tipo string 
        //contendo um comando SQL do tipo SELECT

        // Método para cadastrar uma proposta com seus itens
        public void CadastrarProposta(PropostaModel proposta, List<ItemModel> itens)
        {
            using (MySqlTransaction transaction = Connection.BeginTransaction())
            {
                try
                {
                    // Inserir a proposta
                    string sqlProposta = @"INSERT INTO Propostas (id_clientes, id_vendedor, data, status)
                                           VALUES (@Cliente_id, @Vendedor_id @Data @Status);
                                           SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdProposta = new MySqlCommand(sqlProposta, Connection, transaction);
                    cmdProposta.Parameters.AddWithValue("@Cliente_id", proposta.Cliente_id);
                    cmdProposta.Parameters.AddWithValue("@Vendedor_id", proposta.Vendedor_id);
                    cmdProposta.Parameters.AddWithValue("@Data", proposta.Data);
                    cmdProposta.Parameters.AddWithValue("@Status", proposta.Status);
                    //cmdProposta.Parameters.AddWithValue("@DataFim", proposta.DataFim.HasValue ? (object)proposta.DataFim.Value : DBNull.Value);

                    // Executar o comando e obter o ID da proposta inserida
                    int propostaId = Convert.ToInt32(cmdProposta.ExecuteScalar());

                    // Inserir os itens da proposta
                    foreach (var item in itens)
                    {
                        string sqlItem = @"INSERT INTO Itens (qtde, descricao, preco_unit, id_proposta)
                                           VALUES (@Descricao, @Qtde, @PrecoUnit, @PropostaId);";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, Connection, transaction);
                        cmdItem.Parameters.AddWithValue("@PropostaId", propostaId);
                        cmdItem.Parameters.AddWithValue("@Descricao", item.Descricao);
                        cmdItem.Parameters.AddWithValue("@Qtde", item.Qtde);
                        cmdItem.Parameters.AddWithValue("@PrecoUnit", item.PrecoUnit);

                        cmdItem.ExecuteNonQuery();
                    }

                    // Confirmar a transação
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Desfazer a transação se houver um erro
                    transaction.Rollback();
                    throw;
                }
            }
        }


        public DataTable RetDataTable(string sql)
        {
            DataTable data = new DataTable();
            MySqlCommand Command = new MySqlCommand(sql, Connection);
            MySqlDataAdapter da = new MySqlDataAdapter(Command);
            da.Fill(data);
            return data;
        }

        public DataTable RetDataTable(MySqlCommand Command)
        {
            DataTable data = new DataTable();
            Command.Connection = Connection;
            MySqlDataAdapter da = new MySqlDataAdapter(Command);
            da.Fill(data);
            return data;
        }

        //Espera um parâmetro do tipo string 
        //contendo um comando SQL do tipo INSERT, UPDATE, DELETE
        public void ExecutarComandoSQL(string sql)
        {
            MySqlCommand Command = new MySqlCommand(sql, Connection);
            Command.ExecuteNonQuery();
        }

        public double RetornarDado(string sql)
        {
            double resultado = 0;

            using (MySqlCommand conn = new MySqlCommand(sql, Connection))
            {
                try
                {

                    using (MySqlCommand cmd = new MySqlCommand(sql, Connection))
                    {
                        // Executa o comando e obtém o resultado da soma
                        object valor = cmd.ExecuteScalar();

                        // Verifica se o valor não é nulo e converte para double
                        if (valor != null && valor != DBNull.Value)
                        {
                            resultado = Convert.ToDouble(valor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Tratamento de exceção (opcional: logar o erro ou lidar de outra forma)
                    throw new Exception("Erro ao executar a consulta SQL: " + ex.Message);
                }
            }

            return resultado;

        }
            public int ExecutarConsultaSQL(string sql)
        {
            int resultado = 0;

           
                MySqlCommand Command = new MySqlCommand(sql, Connection);
            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            object valor = Command.ExecuteScalar(); // Executa a consulta e retorna o primeiro valor da primeira linha
                if (valor != null)
                {
                    resultado = Convert.ToInt32(valor); // Converte para inteiro se houver um resultado
                }
            

            return resultado;
        }

    }


}
