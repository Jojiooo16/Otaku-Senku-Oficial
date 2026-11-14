using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
namespace projeto_delivery_2D1
{
    public class Conexao
    {
        public MySqlConnection ConectarBD()
        {
            MySqlConnection conn;
            string dadosConexao = "Persist Security Info = false;" +
                                  "server = localhost; " +
                                  "database = otaku_senku; " +
                                  "uid = root; " +
                                  "pwd =;";

            try
            {
                conn = new MySqlConnection(dadosConexao);
                conn.Open();
                return conn;
            }
            catch (MySqlException ex)
            {

                throw new Exception($"Conexão Não Estabelecida.\n{ex.Message}");
            }

        }





    }
}
